using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using Ninject;
using PoolManager.Monitor.Modules;

namespace PoolManager.Monitor
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceRuntime.RegisterServiceAsync("MonitorType",
                    context => new StandardKernel(new ServiceModule(context)).Get<PoolManagerMonitorService>())
                .GetAwaiter()
                .GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
