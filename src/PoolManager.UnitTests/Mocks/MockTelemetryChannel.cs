using Microsoft.ApplicationInsights.Channel;
using System;
using System.Collections.Generic;

namespace PoolManager.UnitTests.Mocks
{
    public sealed class MockTelemetryChannel : ITelemetryChannel
    {
        private readonly List<ITelemetry> _sentTelemetry;

        public MockTelemetryChannel()
        {
            OnSend = telemetry => { };
            OnFlush = () => { };
            OnDispose = () => { };
            _sentTelemetry = new List<ITelemetry>();
        }

        public IEnumerable<ITelemetry> SentTelemetry => _sentTelemetry;

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public bool ThrowError { get; set; }

        public Action<ITelemetry> OnSend { get; set; }

        public Action OnFlush { get; set; }

        public Action OnDispose { get; set; }

        public void Send(ITelemetry item)
        {
            _sentTelemetry.Add(item);

            if (ThrowError)
            {
                throw new Exception("test error");
            }

            OnSend(item);
        }

        public void Dispose()
        {
            OnDispose();
        }

        public void Flush()
        {
            OnFlush();
        }
    }
}
