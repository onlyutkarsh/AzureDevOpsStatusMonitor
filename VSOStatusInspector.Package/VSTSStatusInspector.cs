//------------------------------------------------------------------------------
// <copyright file="VSTSStatusInspector.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Timers;
using HtmlAgilityPack;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSOStatusInspector
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSOStatusPkgString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    [ProvideOptionPage(typeof(VSOStatusInspectorOptions), EXTENSION_NAME, "General", 0, 0, true)]
    public sealed class VSTSStatusInspector : Package
    {
        private const string EXTENSION_NAME = "VSTS Status Inspector";
        private IVsStatusbar _bar;
        private IntPtr _hdcBitmap = IntPtr.Zero;
        private VSOStatusInspectorOptions _options;
        private Guid _paneGuid = new Guid("{170638A1-CFD7-47C8-975A-FBAA9E532AD5}");
        private IVsOutputWindow _outputWindow;
        private Timer _timer;
        /// <summary>
        /// Initializes a new instance of the <see cref="VSTSStatusInspector"/> class.
        /// </summary>
        public VSTSStatusInspector()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            //Set to unknown icon first
            SetIcon(Resources.unknown);

            //get interval from options
            _options = (VSOStatusInspectorOptions)GetDialogPage(typeof(VSOStatusInspectorOptions));
            if (_options != null)
            {
                _options.OnOptionsChanged += OnOptionsChanged;
            }

            //call the timer code first without waiting for timer trigger
            OnTimerTick(null, null);

            //Set the timer
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromSeconds(_options.Interval).TotalMilliseconds;
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
        }

        private void WriteToOutputWindow(string message)
        {
            IVsOutputWindowPane outputPane;
            OutputWindow.GetPane(ref _paneGuid, out outputPane);

            if (outputPane == null)
            {
                // Create a new pane if not found
                OutputWindow.CreatePane(ref _paneGuid, EXTENSION_NAME, Convert.ToInt32(true), Convert.ToInt32(false));
            }

            // Retrieve the new pane.
            OutputWindow.GetPane(ref _paneGuid, out outputPane);

            outputPane.OutputStringThreadSafe(string.Format("[{0}]\t{1}", DateTime.Now.ToString("hh:mm:ss tt"), message));
            outputPane.OutputStringThreadSafe(Environment.NewLine);
        }

        private void SetIcon(Bitmap icon)
        {
            object oldObject = (object)_hdcBitmap;
            StatusBar.Animation(0, ref oldObject);
            NativeMethods.DeleteObject(_hdcBitmap);

            Bitmap b = ResizeImage(icon, 16);
            _hdcBitmap = b.GetHbitmap();
            object hdcObject = (object)_hdcBitmap;
            StatusBar.Animation(1, ref hdcObject);
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine(string.Format("{0}: Checking VSTS status", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")));

            //set icon to unknown till processing
            SetIcon(Resources.waiting);

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument doc = htmlWeb.Load("https://www.visualstudio.com/en-us/support/support-overview-vs.aspx");

            try
            {
                var div = doc.DocumentNode.SelectSingleNode("//div[@class='TfsServiceStatus']");
                if (div == null)
                {
                    SetIcon(Resources.unknown);
                    WriteToOutputWindow("Could not parse the status. Please visit https://www.visualstudio.com/en-us/support/support-overview-vs.aspx");
                    return;
                }
                var img = div.SelectSingleNode("//img[@id]");
                var h1 = div.SelectSingleNode("//div[@class='RichText']/div[@class='header']/h1");
                var p = div.SelectSingleNode("//div[@class='RichText']/div[@class='header']/p");
                //var img = doc.DocumentNode.SelectSingleNode("//div[@class='TfsServiceStatus']//img[@id]");
                if (img != null)
                {
                    var imageId = img.Id.ToUpper();
                    if (imageId == "GREEN")
                    {
                        SetIcon(Resources.green);
                    }
                    else if (imageId == "YELLOW")
                    {
                        SetIcon(Resources.yellow);
                    }
                    else if (imageId == "RED")
                    {
                        SetIcon(Resources.red);
                    }
                    else
                    {
                        SetIcon(Resources.unknown);
                    }
                    if (h1 != null && p != null)
                    {
                        var msg = string.IsNullOrEmpty(h1.InnerText)
                            ? string.Format("Visual Studio Team Services Status - {0}", p.InnerText)
                            : string.Format("{0} - {1}", h1.InnerText, p.InnerText);
                        WriteToOutputWindow(msg);
                    }
                    else
                    {
                        SetIcon(Resources.unknown);
                        WriteToOutputWindow("Could not parse the status. Please visit https://www.visualstudio.com/en-us/support/support-overview-vs.aspx");
                    }
                }
                else
                {
                    Debug.WriteLine("Found status as Unknown");
                    SetIcon(Resources.unknown);
                }
            }
            catch (Exception exception)
            {
                WriteToOutputWindow("Sorry, an exception occurred.");
                WriteToOutputWindow(exception.ToString());
            }
        }

        private void OnOptionsChanged(object sender, OptionsChangedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(e.Interval).TotalMilliseconds;
            WriteToOutputWindow(String.Format("Interval changed to {0} seconds", e.Interval));
        }
        public IVsOutputWindow OutputWindow
        {
            get
            {
                if (_outputWindow == null)
                {
                    _outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                    return _outputWindow;
                }
                return _outputWindow;
            }
        }
        private IVsStatusbar StatusBar
        {
            get
            {
                if (_bar == null)
                {
                    _bar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
                }

                return _bar;
            }
        }
        public static Bitmap ResizeImage(Bitmap imgToResize, int newHeight)
        {
            //http://stackoverflow.com/questions/25872283/loosing-transparency-in-system-drawing-image-when-using-imageresizer-for-resizin
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercentH = ((float)newHeight / (float)sourceHeight);

            int destWidth = Math.Max((int)Math.Round(sourceWidth * nPercentH), 1); // Just in case;
            int destHeight = newHeight;

            Bitmap bitmap = new Bitmap(destWidth, destHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            }

            return bitmap;
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        #endregion
    }
}
