using System;
using System.Collections.Generic;

namespace VSTSStatusMonitor.Entities
{
    public class VSTSStatusResponse : IDisposable
    {
        public Status Status { get; set; }
        public List<Service> Services { get; set; }

        public void Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Status = null;
                Services = null;
            }
        }
    }

    public class Status
    {
        public string Health { get; set; }
        public string Message { get; set; }
    }

    public class Service
    {
        public string Id { get; set; }
        public List<Geography> Geographies { get; set; }
    }

    public class Geography
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Health { get; set; }
    }

}