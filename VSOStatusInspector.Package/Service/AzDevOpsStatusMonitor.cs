
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using VSTSStatusMonitor.Entities;
using VSTSStatusMonitor.Helpers;

namespace VSTSStatusMonitor.Service
{
    class AzDevOpsStatusMonitor
    {
        public IObservable<VSTSStatusResponse> GetStatusAsync()
        {
            return Observable.FromAsync<VSTSStatusResponse>(async () =>
            {
                var response = new VSTSStatusResponse();
                response.Status = new Status
                {
                    Message = "Polling",
                    Health = "All is good"
                };
                response.LastChecked = DateTime.Now;
                return await Task.FromResult(response);
            });
        }
    }
}
