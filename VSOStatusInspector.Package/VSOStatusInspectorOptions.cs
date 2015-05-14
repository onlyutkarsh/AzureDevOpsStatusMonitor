using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace VSOStatusInspector
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class VSOStatusInspectorOptions : DialogPage
    {
        private int _interval = 60;

        [Category("General")]
        [DisplayName(@"Polling Interval (in seconds)")]
        [Description("Number of seconds between each poll.")]
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
    }
}
