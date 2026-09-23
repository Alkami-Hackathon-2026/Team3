#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using Alkami.Monitoring.Implementation;

namespace Alkami.Monitoring
{
    /// <summary>
    /// Alkami Monitoring provider.
    /// </summary>
    public class AlkamiMonitoring
    {

        internal static Func<IMonitor> MonitorFactory =
            () => new CompositeMonitor(minions, monitorSuppressMonitor.Value);

        private static readonly object Locker = new object();

        internal static readonly List<IMonitorMinion> minions = new List<IMonitorMinion>();

        internal static Lazy<IMonitorSuppressionProvider> monitorSuppressMonitor = new Lazy<IMonitorSuppressionProvider>(() => new MonitorSuppressionProvider());


        /// <summary>
        /// Sets up Alkami monitoring.
        /// </summary>
        public static void SetUp<TImplementation>(TImplementation implementation)
            where TImplementation : class, IMonitorMinion
        {
            lock (Locker)
            {
                Metric.ResetMonitor();
                Metric.MonitorFactory ??= MonitorFactory;
                if (!minions.Contains(implementation))
                {
                    minions.Add(implementation);
                }
            }
        }

        /// <summary>
        /// Removes all minions of the type of <typeparamref name="TImplementation"/>
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        public static void RemoveType<TImplementation>()
            where TImplementation : class, IMonitorMinion
        {
            lock (Locker)
            {
                var find = minions.Where(x => x is TImplementation).Select(x => x).ToList();
                foreach (var value in find)
                {
                    if (value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    minions.Remove(value);
                }
            }
        }
    }
}
#endif
