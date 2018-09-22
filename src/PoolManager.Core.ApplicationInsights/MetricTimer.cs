using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PoolManager.Core.ApplicationInsights
{
    public static class MetricExtensions
    {
        public static MetricTimer TrackMetricTimer(this TelemetryClient telemetry, string metricId, string dimension1Name = null, string dimension1Value = null)
        {
            return new MetricTimer(telemetry, metricId, dimension1Name, dimension1Value);
        }
    }

    public class MetricTimer : IDisposable
    {
        private readonly TelemetryClient _telemetry;
        private readonly Stopwatch _timer;
        private readonly string _metricId;
        private readonly string _dimension1Name;
        private readonly string _dimension1Value;

        public MetricTimer(TelemetryClient telemetry, string metricId, string dimension1Name = null, string dimension1Value = null)
        {
            _metricId = metricId;
            _dimension1Name = dimension1Name;
            _dimension1Value = dimension1Value;
            _telemetry = telemetry;
            _timer = new Stopwatch();
            _timer.Start();
            _telemetry.TrackTrace(_metricId + ".Began");
        }

        public void Dispose()
        {
            var elapsed = _timer.Elapsed;
            _telemetry.TrackTrace(_metricId + ".Completed", new Dictionary<string, string> {{"duration", elapsed.ToString()}});
            if(string.IsNullOrEmpty(_dimension1Name))
                _telemetry.GetMetric(_metricId).TrackValue(elapsed.TotalMilliseconds);
            else
                _telemetry.GetMetric(_metricId, _dimension1Name).TrackValue(elapsed.TotalMilliseconds, _dimension1Value);
        }
    }
}
