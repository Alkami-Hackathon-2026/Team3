using Alkami.Data.Validations;
using Alkami.Services.Subscriptions.Data;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// Temporary class until we upgrade to 4.0 to prevent any existing signature changes
    /// </summary>
    public class ServiceDefinitionWindowData
    {
        private static readonly ILog _logger = LogManager.GetLogger<ServiceDefinitionWindowData>();

        private const int SlidingWindowSize = 3;

        private List<ServiceDefinitionSnapshot> Snapshots { get; set; } = new List<ServiceDefinitionSnapshot>(SlidingWindowSize + 1);

        private class ServiceDefinitionSnapshot
        {
            public ServiceDefinitionSnapshot(ServiceDefinition definition)
            {
                AverageMessageDuration = 10d;
                ErrorRatio = 0.01;
                MessageCount = 0;
            }

            public double AverageMessageDuration { get; set; }

            public double ErrorRatio { get; set; }

            public int MessageCount { get; set; }

            public DateTimeOffset Uptime { get; set; }
        }

        public ServiceDefinitionWindowData(Uri endpointUri)
        {
            AverageMessageDuration = 10; // 10 ms
            AverageErrorRatio = 0.01; // 1 per 100 calls

            EndpointUri = endpointUri;
        }

        public double AverageMessageDuration { get; set; }

        public double AverageErrorRatio { get; set; }

        public Uri EndpointUri { get; set; }

        public double AverageMessageCount { get; set; }

        public void AdjustWindow(ServiceDefinition serviceDefinition)
        {
            // This method currently depends on the lock that is being used in SelfResolvingClient on line 214. If that lock is removed
            // or the call to this method is moved out of that lock, we need to add a lock for the Snapshots list insertion/removal

            var snapshot = new ServiceDefinitionSnapshot(serviceDefinition);

            Snapshots.Insert(0, snapshot);

            if (Snapshots.Count > SlidingWindowSize)
            {
                Snapshots.RemoveAt(SlidingWindowSize);
            }

            // Response time
            var averageMessageDuration = Snapshots.Average(x => x.AverageMessageDuration);

            if (averageMessageDuration < 10d)
            {
                averageMessageDuration = 10d; // 10ms is the minimum
            }

            AverageMessageDuration = averageMessageDuration;

            // Error ratio
            var averageErrorRatio = Snapshots.Average(x => x.ErrorRatio);

            if (averageErrorRatio < 0.01d)
            {
                averageErrorRatio = 0.01;
            }

            AverageErrorRatio = averageErrorRatio;

            // Message count
            AverageMessageCount = Snapshots.Average(x => x.MessageCount);

            if (_logger.IsTraceEnabled)
                _logger.Trace($"ServiceDefinitionWindowData updated: EndpointUri: {EndpointUri}, AverageMessageCount: {AverageMessageCount}, " +
                    $"AverageMessageDuration: {AverageMessageDuration}, AverageErrorRatio: {AverageErrorRatio}");
        }

        public override string ToString()
        {
            return $"ServiceDefinitionWindowData updated: EndpointUri: {EndpointUri}, AverageMessageCount: {AverageMessageCount}, " +
                    $"AverageMessageDuration: {AverageMessageDuration}, AverageErrorRatio: {AverageErrorRatio}";
        }
    }
}
