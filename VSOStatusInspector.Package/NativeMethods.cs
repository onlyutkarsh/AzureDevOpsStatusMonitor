using System;
using System.Runtime.InteropServices;

namespace VSTSStatusMonitor
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
