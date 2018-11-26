using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using VSTSStatusMonitor.Entities;
using VSTSStatusMonitor.Helpers;
using VSTSStatusMonitor.Service;
using Task = System.Threading.Tasks.Task;

namespace VSTSStatusMonitor
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSOStatusPkgString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideBindingPath]
    [ProvideOptionPage(typeof(Options), EXTENSION_NAME, "General", 0, 0, true)]
    public sealed class VSTSStatusMonitorPackage : AsyncPackage
    {
        private const string EXTENSION_NAME = "VSTS Status Monitor";
        private IVsStatusbar _bar;
        private IntPtr _hdcBitmap = IntPtr.Zero;
        private static Options _options;
        private Guid _paneGuid = new Guid("{170638A1-CFD7-47C8-975A-FBAA9E532AD5}");
        private IVsOutputWindow _outputWindow;
        private System.Timers.Timer _timer;
        private static readonly object _syncRoot = new object();
        AzDevOpsStatusMonitor _monitor = new AzDevOpsStatusMonitor();
        private IDisposable _subscription;


        public static Options Options
        {
            get
            {
                if (_options == null)
                {
                    lock (_syncRoot)
                    {
                        if (_options == null)
                        {
                            LoadPackage();
                        }
                    }
                }

                return _options;
            }
        }


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Switches to the UI thread in order to consume some services used in command initialization
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            //Set to unknown icon first
            //SetIcon(Resources.unknown);

            //get interval from options
            _options = (Options)GetDialogPage(typeof(Options));

            Logger.Initialize(this, Vsix.Name);

            PollForStatus();


            ////call the timer code first without waiting for timer trigger
            //OnTimerTick(null, null);

            ////Set the timer
            _timer = new System.Timers.Timer();
            //_timer.Interval = TimeSpan.FromSeconds(_options.Interval).TotalMilliseconds;
            //_timer.Elapsed += OnTimerTick;
            //_timer.Start();

        }

        private void PollForStatus()
        {
            if (_options != null)
            {
                _options.OnOptionsChanged += OnOptionsChanged;
            }

            var interval = _options?.Interval ?? 5;

            // https://github.com/LeeCampbell/RxCookbook/blob/master/Repository/Polling.md
            _subscription = _monitor.Poll<VSTSStatusResponse>(TimeSpan.FromSeconds(interval))
                .Subscribe(res =>
                {
                    res.Switch(r => { Logger.Log(r.Status.Message); },
                        e => { Logger.Log(e.Message); });
                });
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

        //private void OnTimerTick(object sender, ElapsedEventArgs e)
        //{
        //    Debug.WriteLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Checking VSTS status");
        //    try
        //    {
        //        //set icon to unknown till processing
        //        //SetIcon();

        //        using (var client = new HttpClient())
        //        {
        //            var response = client.GetAsync("https://www.visualstudio.com/wp-json/vscom/v1/service-status").GetAwaiter()
        //                .GetResult();

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                //SetIcon(Resources.unknown);
        //                WriteToOutputWindow("Could not parse the status. Please visit https://www.visualstudio.com/team-services/support");
        //                return;
        //            }
        //            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        //            var vstsResponse = JsonConvert.DeserializeObject<VSTSStatusResponse>(content);


        //            if (string.Equals("maintenance", vstsResponse.Status, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                //SetIcon(Resources.red);
        //                WriteToOutputWindow($"{vstsResponse.Title} - {vstsResponse.Message}. Please visit https://blogs.msdn.com/b/vsoservice/ for details and history");

        //            }
        //            else if (string.Equals("available", vstsResponse.Status, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                //SetIcon(Resources.green);
        //                WriteToOutputWindow($"{vstsResponse.Title} - {vstsResponse.Message}.");

        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        WriteToOutputWindow("Sorry, an exception occurred.");
        //        WriteToOutputWindow(exception.ToString());
        //    }
        //}

        private void OnOptionsChanged(object sender, OptionsChangedEventArgs e)
        {
            //_timer.Interval = TimeSpan.FromSeconds(e.Interval).TotalMilliseconds;
            //WriteToOutputWindow(String.Format("Interval changed to {0} seconds", e.Interval));
            if (_subscription != null)
            {
                _subscription.Dispose();
            }
            PollForStatus();
        }
        public IVsOutputWindow OutputWindow
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
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

        private static void LoadPackage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var shell = (IVsShell)GetGlobalService(typeof(SVsShell));

            if (shell.IsPackageLoaded(ref GuidList.guidVSOStatusPkg, out IVsPackage package) != VSConstants.S_OK)
            {
                ErrorHandler.Succeeded(shell.LoadPackage(ref GuidList.guidVSOStatusPkg, out package));
            }
        }
    }
}