
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using VSTSStatusMonitor.Entities;

namespace VSTSStatusMonitor.Service
{
    class AzDevOpsStatusMonitor : IObservable<VSTSStatusResponse>
    {
        public IDisposable Subscribe(IObserver<VSTSStatusResponse> observer)
        {
            var response = new VSTSStatusResponse();
            response.Status = new Status();
            response.Status.Message = "Polling";
            observer.OnNext(response);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}
