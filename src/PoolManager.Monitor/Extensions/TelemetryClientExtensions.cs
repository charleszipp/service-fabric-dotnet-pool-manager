using System.Collections.Generic;
using Microsoft.ApplicationInsights;

namespace PoolManager.Monitor.Extensions
{
    public static class TelemetryClientExtensions
    {
        public static void TrackStateChangeEvents(this TelemetryClient telemetryClient, string resourceName, string newState)
        {
            var properties = new Dictionary<string, string>
            {
                {"EventName", $"Service Pool Monitor resource state change."},
                {"ResourceName", resourceName},
                {"StateChange", newState}
            };
            telemetryClient.TrackEvent($"{resourceName} changing states to {newState}.", properties);
        }

        public static void TrackThreadState(this TelemetryClient telemetryClient, string threadName, string newState)
        {
            var properties = new Dictionary<string, string>
            {
                {"EventName", $"Service Pool Monitor thread state change."},
                {"Thread", threadName},
                {"StateChange", newState}
            };
            telemetryClient.TrackEvent($"{threadName} changing states to {newState}.", properties);
        }
    }
}