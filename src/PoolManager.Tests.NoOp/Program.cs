using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;

namespace PoolManager.Tests.NoOp
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceRuntime.RegisterServiceAsync("NoOp",
                context => new NoOp(context)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}