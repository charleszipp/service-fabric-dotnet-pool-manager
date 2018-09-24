using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;

namespace PoolManager.Web
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceRuntime.RegisterServiceAsync("WebType",
                    context => new Web(context)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}