using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace VSTSStatusMonitor
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class Options : DialogPage
    {
        private int _interval = 60;

        [Category("General")]
        [DisplayName(@"Polling Interval (in seconds)")]
        [Description("Number of seconds between each poll.")]
        [DefaultValue(300)]
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
#if !DEBUG
            if (Interval < 300)
            {
                MessageBox.Show("Interval cannot be less than 300 seconds (5 min)", Vsix.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                Interval = 300;
                e.ApplyBehavior = ApplyKind.Cancel;
            }
#else

            Interval = 10;
            e.ApplyBehavior = ApplyKind.Apply;
#endif
            base.OnApply(e);
            if (OnOptionsChanged != null)
            {
                var optionsEventArg = new OptionsChangedEventArgs
                {
                    Interval = Interval,
                };
                OnOptionsChanged(this, optionsEventArg);
            }
        }

        public event EventHandler<OptionsChangedEventArgs> OnOptionsChanged;
    }
}
