using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using VSTSStatusMonitor.Entities;

namespace VSTSStatusMonitor
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSOStatusPkgString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    [ProvideOptionPage(typeof(VSTSStatusMonitorOptions), EXTENSION_NAME, "General", 0, 0, true)]
    public sealed class VSTSStatusMonitorPackage : Package
    {
        private const string EXTENSION_NAME = "VSTS Status Monitor";
        private IVsStatusbar _bar;
        private IntPtr _hdcBitmap = IntPtr.Zero;
        private VSTSStatusMonitorOptions _options;
        private Guid _paneGuid = new Guid("{170638A1-CFD7-47C8-975A-FBAA9E532AD5}");
        private IVsOutputWindow _outputWindow;
        private Timer _timer;

        protected override void Initialize()
        {
            base.Initialize();
            //Set to unknown icon first
            SetIcon(Resources.unknown);

            //get interval from options
            _options = (VSTSStatusMonitorOptions)GetDialogPage(typeof(VSTSStatusMonitorOptions));
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

            outputPane.OutputStringThreadSafe($"[{DateTime.Now:hh:mm:ss tt}]\t{message}");
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
            Debug.WriteLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Checking VSTS status");
            try
            {
                //set icon to unknown till processing
                SetIcon(Resources.waiting);

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://www.visualstudio.com/wp-json/vscom/v1/service-status").GetAwaiter()
                        .GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        SetIcon(Resources.unknown);
                        WriteToOutputWindow("Could not parse the status. Please visit https://www.visualstudio.com/team-services/support");
                        return;
                    }
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var vstsResponse = JsonConvert.DeserializeObject<VSTSStatusResponse>(content);


                    if (string.Equals("maintenance", vstsResponse.Status, StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetIcon(Resources.red);
                        WriteToOutputWindow($"{vstsResponse.Title} - {vstsResponse.Message}. Please visit https://blogs.msdn.com/b/vsoservice/ for details and history");

                    }
                    else if (string.Equals("available", vstsResponse.Status, StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetIcon(Resources.green);
                        WriteToOutputWindow($"{vstsResponse.Title} - {vstsResponse.Message}.");

                    }
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
    }
}