using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#if NET6_0_OR_GREATER
using Microsoft.Extensions.Logging;
#elif NETFRAMEWORK
using Common.Logging;
#endif

namespace Alkami.Monitoring.Implementation
{
    /// <summary>
    /// The default Monitor which supports multiple minion implementations.
    /// </summary>
    internal class CompositeMonitor : IMonitor, IMonitorProperties, IMonitorSuppression
    {
        private readonly ConcurrentDictionary<string, IMonitorMinion> _minions;
        private readonly IMonitorSuppressionProvider _suppressionProvider;

#if NETFRAMEWORK
        private readonly ILog _logger = LogManager.GetLogger<CompositeMonitor>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minions"></param>
        /// <param name="suppressionProvider"></param>
        public CompositeMonitor(IEnumerable<IMonitorMinion> minions, IMonitorSuppressionProvider suppressionProvider)
        {
            _minions = new ConcurrentDictionary<string, IMonitorMinion>();
            foreach (var minion in minions)
            {
                var alias = MonitorMinionAliasUtilities.GetAlias(minion.GetType()).ToLower();
                _minions.TryAdd(alias, minion);
            }
            _suppressionProvider = suppressionProvider;
        }
#endif

#if NET6_0_OR_GREATER
        private readonly ILogger<CompositeMonitor> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="minions"></param>
        /// <param name="suppressionProvider"></param>
        public CompositeMonitor(ILogger<CompositeMonitor> logger, IEnumerable<IMonitorMinion> minions, IMonitorSuppressionProvider suppressionProvider)
        {
            _logger = logger;

            _minions = new ConcurrentDictionary<string, IMonitorMinion>();
            foreach (var minion in minions)
            {
                var alias = MonitorMinionAliasUtilities.GetAlias(minion.GetType()).ToLower();
                _minions.TryAdd(alias, minion);
            }

            _suppressionProvider = suppressionProvider;
        }
#endif

        /// <inheritdoc cref="IMonitor"/>
        public string AreaName
        {
            get => Metric.AreaName;
            set => Metric.AreaName = value;
        }

        private void SafeMinionInvoke(Action<IMonitorMinion> action)
        {
            HashSet<string> suppressTypes = new();
            _suppressionProvider.ForEachScope((scope, state) =>
            {
                state.Add(scope.ToLower());
            }, suppressTypes);

            foreach (var minion in _minions)
            {
                if (suppressTypes.Contains(minion.Key))
                {
                    continue;
                }
                try
                {
                    action(minion.Value);
                }
                catch (Exception error)
                {
#if NET6_0_OR_GREATER
                    _logger.LogError(error, "Error invoking minion");
#elif NETFRAMEWORK
                    _logger.Error("Error invoking minion", error);
#endif
                }
            }
        }

        /// <inheritdoc cref="IMonitor"/>
        public string CreateName(params string[] orderedSegments)
        {
            return Metric.CreateName(orderedSegments);
        }

        /// <inheritdoc cref="IMonitor"/>
        public void IncrementCounter(string name)
        {
            SafeMinionInvoke(x => x.IncrementCounter(name));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void IncrementCounter(params string[] orderedSegments)
        {
            SafeMinionInvoke(x => x.IncrementCounter(orderedSegments));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void NoticeError(Exception exception)
        {
            SafeMinionInvoke(x => x.NoticeError(exception));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void NoticeError(Exception exception, IDictionary<string, string> parameters)
        {
            SafeMinionInvoke(x => x.NoticeError(exception, parameters));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void NoticeError(string message, IDictionary<string, string> parameters)
        {
            SafeMinionInvoke(x => x.NoticeError(message, parameters));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void RecordMetric(string name, float value)
        {
            SafeMinionInvoke(x => x.RecordMetric(name, value));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void RecordMetric(float value, params string[] orderedSegments)
        {
            SafeMinionInvoke(x => x.RecordMetric(value, orderedSegments));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void RecordResponseTimeMetric(string name, TimeSpan duration)
        {
            SafeMinionInvoke(x => x.RecordResponseTimeMetric(name, duration));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void RecordResponseTimeMetric(TimeSpan duration, params string[] orderedSegments)
        {
            SafeMinionInvoke(x => x.RecordResponseTimeMetric(duration, orderedSegments));
        }

        /// <inheritdoc cref="IMonitor"/>
        public void RecordCustomEvent(string name, MonitorEvent payload)
        {
            SafeMinionInvoke(x => x.RecordCustomEvent(name, payload));
        }

        /// <inheritdoc cref="IMonitorProperties"/>
        public void AddCustomProperty(string name, string value)
        {
            SafeMinionInvoke(x => x.AddCustomProperty(name, value));
        }

        /// <inheritdoc cref="IMonitorProperties"/>
        public void IgnoreTransaction()
        {
            SafeMinionInvoke(x => x.IgnoreTransaction());
        }

        /// <inheritdoc cref="IMonitorProperties"/>
        public void SetTransactionName(string category, string name)
        {
            SafeMinionInvoke(x => x.SetTransactionName(category, name));
        }

        /// <inheritdoc cref="IMonitorProperties"/>
        public void SetUserProperties(string userValue, string accountValue, string productValue)
        {
            SafeMinionInvoke(x => x.SetUserProperties(userValue, accountValue, productValue));
        }

        public IDisposable BeginSuppression(string metricType)
        {
            return _suppressionProvider.Push(metricType);
        }
    }
}
