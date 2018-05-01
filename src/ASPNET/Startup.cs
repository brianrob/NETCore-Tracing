using System;
using Microsoft.AspNetCore.Hosting;

// Identify the IHostingStartup implementation.
[assembly: HostingStartup(typeof(NETCore.Tracing.Service.ASPNET.TracingHostingStartup))]

namespace NETCore.Tracing.Service.ASPNET
{
    public sealed class TracingHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            Console.WriteLine("TracingHostingStartup.Configure");
            NETCore.Tracing.Service.Control.Start();
        }
    }
}
