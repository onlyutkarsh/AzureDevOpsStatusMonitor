using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using VSTSStatusMonitor.Entities;

namespace VSTSStatusMonitor.UI
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

        private void VSTSStatusChanged(object sender, VSTSStatusResponse response)
        {
            string statusMessage = "Checking Azure DevOps status...";
            if (response != null && response.Status != null)
            {

            }
            else
            {
                imgStatus.Moniker = KnownMonikers.StatusInvalidOutlineNoColor;
            }
            txtOverallStatus.Text = response.Status.Message;
            lastChecked.Text = response.LastChecked.ToString();
        }
    }
}
