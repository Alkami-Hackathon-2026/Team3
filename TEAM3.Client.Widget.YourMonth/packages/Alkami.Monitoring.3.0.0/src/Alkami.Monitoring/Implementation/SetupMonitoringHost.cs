#if NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Alkami.Monitoring.Implementation
{
    /// <summary>
    /// Sets up the Monitor Factory when initialized.
    /// </summary>
    internal class SetupMonitoringHost : BackgroundService
    {
        public SetupMonitoringHost(CompositeMonitor monitor)
        {
            Metric.ResetMonitor();
            Metric.MonitorFactory = () => monitor;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
#endif
