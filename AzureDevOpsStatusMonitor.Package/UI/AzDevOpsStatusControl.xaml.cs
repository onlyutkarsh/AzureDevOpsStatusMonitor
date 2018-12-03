using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using AzureDevOpsStatusMonitor.Entities;

namespace AzureDevOpsStatusMonitor.UI
{
    /// <summary>
    /// Interaction logic for AzDevOpsStatus.xaml
    /// </summary>
    public partial class AzDevOpsStatusControl : UserControl
    {
        private AzureDevOpsStatusMonitorPackage _packaage;

        public AzDevOpsStatusControl(AzureDevOpsStatusMonitorPackage azureDevOpsStatusMonitorPackage)
        {
            _packaage = azureDevOpsStatusMonitorPackage;
            _packaage.OnStatusChanged += VSTSStatusChanged;
            InitializeComponent();
        }

        private void VSTSStatusChanged(object sender, VSTSStatusResponse response)
        {
            lastChecked.Text = $"Last Checked: {DateTime.Now}";
            if (response?.Status != null)
            {
                var monikerBasedOnHealth = GetMonikerBasedOnHealth(response.Status.Health);
                imgStatusMid.Moniker = monikerBasedOnHealth;
                imgStatusTop.Moniker = monikerBasedOnHealth;
                txtOverallStatus.Text = response.Status.Message;
            }
            else
            {
                txtOverallStatus.Text = "Unable to get Azure DevOps status";
                imgStatusMid.Moniker = KnownMonikers.StatusInvalidOutline;
                imgStatusTop.Moniker = KnownMonikers.StatusInvalidOutline;
            }
            
        }

        private ImageMoniker GetMonikerBasedOnHealth(string statusHealth)
        {
            switch (statusHealth.ToLower())
            {
                case "healthy":
                {
                    return MyMonikers.Healthy;
                }
                case "degraded":
                {
                    return MyMonikers.Degraded;
                }
                case "unhealthy":
                {
                        return MyMonikers.Unhealthy;
                }
                case "advisory":
                {
                    return MyMonikers.Advisory;
                }
                default:
                {
                    return KnownMonikers.StatusInvalidOutline;
                }
            }
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            popUp.IsOpen = true;
        }
    }
}
