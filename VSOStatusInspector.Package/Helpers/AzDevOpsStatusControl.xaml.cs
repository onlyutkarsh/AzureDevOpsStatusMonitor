using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSTSStatusMonitor.Helpers
{
    /// <summary>
    /// Interaction logic for AzDevOpsStatus.xaml
    /// </summary>
    public partial class AzDevOpsStatusControl : UserControl
    {
        private VSTSStatusMonitorPackage _packaage;

        public AzDevOpsStatusControl(VSTSStatusMonitorPackage vstsStatusMonitorPackage)
        {
            _packaage = vstsStatusMonitorPackage;
            _packaage.OnStatusChanged += VSTSStatusChanged;
            InitializeComponent();
        }

        private void VSTSStatusChanged(object sender, string e)
        {
            lastChecked.Text = e;
        }
    }
}
