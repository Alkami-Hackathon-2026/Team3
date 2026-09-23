using System;
using System.Diagnostics;
using System.Threading;

namespace Alkami.Monitoring.Implementation
{
    /// <summary>
    /// Default implementation of <see cref="IMonitorSuppressionProvider"/>
    /// </summary>
    internal class MonitorSuppressionProvider : IMonitorSuppressionProvider
    {
        private readonly AsyncLocal<Scope> _currentScope = new AsyncLocal<Scope>();

        /// <inheritdoc />
        public void ForEachScope<TState>(Action<string, TState> callback, TState state)
        {
            void Report(Scope current)
            {
                if (current == null)
                {
                    return;
                }
                Report(current.Parent);
                callback(current.MetricType, state);
            }
            Report(_currentScope.Value);
        }

        /// <inheritdoc />
        public IDisposable Push(string metricType)
        {
            Scope parent = _currentScope.Value;
            var newScope = new Scope(this, metricType, parent);
            _currentScope.Value = newScope;

            return newScope;
        }


        private sealed class Scope : IDisposable
        {
            private readonly MonitorSuppressionProvider _provider;
            private bool _isDisposed;

            internal Scope(MonitorSuppressionProvider provider, string metricType, Scope parent)
            {
                _provider = provider;
                MetricType = metricType;
                Parent = parent;
            }

            public Scope Parent { get; }

            public string MetricType { get; }

            public override string ToString()
            {
                return MetricType?.ToString();
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _provider._currentScope.Value = Parent;
                    _isDisposed = true;
                }
            }
        }
    }

    internal readonly struct ScopeMonitorSuppression
    {
        public ScopeMonitorSuppression(IMonitorSuppression monitorSuppression, IMonitorSuppressionProvider externalScopeProvider)
        {
            Debug.Assert(monitorSuppression != null || externalScopeProvider != null, "MonitorSuppression can't be null when there isn't an ExternalScopeProvider");

            MonitorSuppression = monitorSuppression;
            ExternalScopeProvider = externalScopeProvider;
        }

        public IMonitorSuppression MonitorSuppression { get; }

        public IMonitorSuppressionProvider ExternalScopeProvider { get; }

        public IDisposable CreateSuppression(string metricType)
        {
            if (ExternalScopeProvider != null)
            {
                return ExternalScopeProvider.Push(metricType);
            }

            Debug.Assert(MonitorSuppression != null);
            return MonitorSuppression.BeginSuppression(metricType);
        }
    }

}
