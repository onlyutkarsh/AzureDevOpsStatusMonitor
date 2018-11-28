
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Reactive.Linq;
using VSTSStatusMonitor.Entities;

namespace VSTSStatusMonitor.Service
{
    class AzDevOpsStatusMonitor
    {
        public IObservable<VSTSStatusResponse> GetStatusAsync()
        {
            return Observable.FromAsync<VSTSStatusResponse>(async () =>
            {
                var response = new VSTSStatusResponse();
                response.Status = new Status();
                response.LastChecked = DateTime.Now;

                using (HttpClient client = new HttpClient())
                {
                    var rawResponse = await client.GetAsync("https://status.dev.azure.com/_apis/status/health").ConfigureAwait(false);
                    if (!rawResponse.IsSuccessStatusCode)
                    {
                        response.Status.Health = "Unknown error occurred";
                        response.Status.Message = "Error occurred here";
                        return response;
                    }

                    var content = await rawResponse.Content.ReadAsStringAsync();

                    response = JsonConvert.DeserializeObject<VSTSStatusResponse>(content);
                }


                return response;
            });
        }
    }
}
