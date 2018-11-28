using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using VSTSStatusMonitor.Entities;

namespace VSTSStatusMonitor.Helpers
{
    /// <summary>
    /// Interaction logic for AzDevOpsStatus.xaml
    /// </summary>
    public partial class AzDevOpsStatusControl : UserControl
    {
        private VSTSStatusMonitorPackage _packaage;
        Dictionary<string, List<Geography>> _servicesDictionary = new Dictionary<string, List<Geography>>();

        public AzDevOpsStatusControl(VSTSStatusMonitorPackage vstsStatusMonitorPackage)
        {
            _packaage = vstsStatusMonitorPackage;
            _packaage.OnStatusChanged += VSTSStatusChanged;
            InitializeComponent();
        }

        private void VSTSStatusChanged(object sender, VSTSStatusResponse response)
        {
            foreach (var service in response.Services)
            {
                if (!_servicesDictionary.ContainsKey(service.Id))
                {
                    _servicesDictionary.Add(service.Id, service.Geographies);
                }
                else
                {
                    _servicesDictionary[service.Id] = service.Geographies;
                }
            }



            foreach (KeyValuePair<string, List<Geography>> keyValuePair in _servicesDictionary)
            {
                var geographies = keyValuePair.Value;
                var usa = geographies.First(x => string.Equals("US", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var ca = geographies.First(x => string.Equals("CA", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var br = geographies.First(x => string.Equals("BR", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var eu = geographies.First(x => string.Equals("EU", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var apac = geographies.First(x => string.Equals("APAC", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var au = geographies.First(x => string.Equals("AU", x.Id, StringComparison.InvariantCultureIgnoreCase));
                var ind = geographies.First(x => string.Equals("IN", x.Id, StringComparison.InvariantCultureIgnoreCase));

                lblUS.Content = usa.Name;
                lblCA.Content = ca.Name;
                lblBR.Content = br.Name;
                lblEU.Content = eu.Name;
                lblAPAC.Content = apac.Name;
                lblAU.Content = au.Name;
                lblIN.Content = ind.Name;
                txtOverallStatus.Text = response.Status.Message;
                lastChecked.Text = response.LastChecked.ToString();

                switch (keyValuePair.Key.ToLower())
                {
                    case "core services":
                        {
                            lblCoreService.Content = keyValuePair.Key;
                            imgCoreUS.Moniker = GetMoniker(usa.Health);
                            imgCoreCA.Moniker = GetMoniker(ca.Health);
                            imgCoreBR.Moniker = GetMoniker(br.Health);
                            imgCoreEU.Moniker = GetMoniker(eu.Health);
                            imgCoreAPAC.Moniker = GetMoniker(apac.Health);
                            imgCoreAU.Moniker = GetMoniker(au.Health);
                            imgCoreIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "boards":
                        {
                            lblBoards.Content = keyValuePair.Key;
                            imgBoardsUS.Moniker = GetMoniker(usa.Health);
                            imgBoardsCA.Moniker = GetMoniker(ca.Health);
                            imgBoardsBR.Moniker = GetMoniker(br.Health);
                            imgBoardsEU.Moniker = GetMoniker(eu.Health);
                            imgBoardsAPAC.Moniker = GetMoniker(apac.Health);
                            imgBoardsAU.Moniker = GetMoniker(au.Health);
                            imgBoardsIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "repos":
                        {
                            lblRepos.Content = keyValuePair.Key;
                            imgReposUS.Moniker = GetMoniker(usa.Health);
                            imgReposCA.Moniker = GetMoniker(ca.Health);
                            imgReposBR.Moniker = GetMoniker(br.Health);
                            imgReposEU.Moniker = GetMoniker(eu.Health);
                            imgReposAPAC.Moniker = GetMoniker(apac.Health);
                            imgReposAU.Moniker = GetMoniker(au.Health);
                            imgReposIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "pipelines":
                        {
                            lblPipelines.Content = keyValuePair.Key;
                            imgPipelinesUS.Moniker = GetMoniker(usa.Health);
                            imgPipelinesCA.Moniker = GetMoniker(ca.Health);
                            imgPipelinesBR.Moniker = GetMoniker(br.Health);
                            imgPipelinesEU.Moniker = GetMoniker(eu.Health);
                            imgPipelinesAPAC.Moniker = GetMoniker(apac.Health);
                            imgPipelinesAU.Moniker = GetMoniker(au.Health);
                            imgPipelinesIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "test plans":
                        {
                            lblTestPlans.Content = keyValuePair.Key;
                            imgTestPlansUS.Moniker = GetMoniker(usa.Health);
                            imgTestPlansCA.Moniker = GetMoniker(ca.Health);
                            imgTestPlansBR.Moniker = GetMoniker(br.Health);
                            imgTestPlansEU.Moniker = GetMoniker(eu.Health);
                            imgTestPlansAPAC.Moniker = GetMoniker(apac.Health);
                            imgTestPlansAU.Moniker = GetMoniker(au.Health);
                            imgTestPlansIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "artifacts":
                        {
                            lblArtifacts.Content = keyValuePair.Key;
                            imgArtifactsUS.Moniker = GetMoniker(usa.Health);
                            imgArtifactsCA.Moniker = GetMoniker(ca.Health);
                            imgArtifactsBR.Moniker = GetMoniker(br.Health);
                            imgArtifactsEU.Moniker = GetMoniker(eu.Health);
                            imgArtifactsAPAC.Moniker = GetMoniker(apac.Health);
                            imgArtifactsAU.Moniker = GetMoniker(au.Health);
                            imgArtifactsIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                    case "other services":
                        {
                            lblOtherServices.Content = keyValuePair.Key;
                            imgOthersUS.Moniker = GetMoniker(usa.Health);
                            imgOthersCA.Moniker = GetMoniker(ca.Health);
                            imgOthersBR.Moniker = GetMoniker(br.Health);
                            imgOthersEU.Moniker = GetMoniker(eu.Health);
                            imgOthersAPAC.Moniker = GetMoniker(apac.Health);
                            imgOthersAU.Moniker = GetMoniker(au.Health);
                            imgOthersIN.Moniker = GetMoniker(ind.Health);

                            break;
                        }
                }
            }

        }

        private ImageMoniker GetMoniker(string geographyHealth)
        {
            switch (geographyHealth.ToLower())
            {
                case "healthy":
                    {
                        return KnownMonikers.StatusOK;
                    }
                case "degraded":
                    {
                        return KnownMonikers.StatusWarning;

                    }
                case "unhealthy":
                    {
                        return KnownMonikers.StatusError;
                    }
                case "advisory":
                    {
                        return KnownMonikers.StatusInformation;
                    }
                default:
                    {
                        return KnownMonikers.StatusInformationNoColor;
                    }
            }
        }
    }
}
