using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Monitoring
{
    /// <summary>
    /// The Metric class abstracts out calls to the application's <see cref="IMonitor"/> instance.
    /// </summary>
    public static class Metric
    {
        /// <summary>
        /// The separator used for each segment of a metric.
        /// </summary>
        public static readonly char Separator = '/';

        /// <summary>
        /// Factory for creating the <see cref="IMonitor"/> instance.
        /// </summary>
        public static Func<IMonitor> MonitorFactory = null;

        internal static IMonitor Monitor;
        internal static IMonitorProperties MonitorProperties;

        private static string _areaName = "Unknown";

        /// <summary>
        /// The name of the area for metrics collection.
        /// </summary>
        public static string AreaName
        {
            get
            {
                return CanMonitor() ? Monitor.AreaName : _areaName;
            }
            set
            {
                // Remove any separator characters.
                _areaName = value.Replace(Separator, '-');
                if (CanMonitor())
                {
                    Monitor.AreaName = _areaName;
                }
            }
        }

        /// <summary>
        /// Creates a consistent metric name for the metrics collection tool.
        /// </summary>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        /// <returns>The complete name for the metric.</returns>
        public static string CreateName(params string[] orderedSegments)
        {
            if (CanMonitor())
                return Monitor.CreateName(orderedSegments);

            return string.Join(Separator.ToString(), new[] { "Custom", AreaName }.Concat(orderedSegments));
        }

        /// <summary>
        /// Increments a metric counter.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        public static void IncrementCounter(string name)
        {
            if (CanMonitor())
                Monitor.IncrementCounter(name);
        }

        /// <summary>
        /// Increments a metric counter.
        /// </summary>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        public static void IncrementCounter(params string[] orderedSegments)
        {
            if (CanMonitor())
                Monitor.IncrementCounter(orderedSegments);
        }

        /// <summary>
        /// Notices an error
        /// </summary>
        /// <param name="exception">The exception to notice</param>
        public static void NoticeError(Exception exception)
        {
            if (CanMonitor())
                Monitor.NoticeError(exception);
        }

        /// <summary>
        ///  Notices an error
        /// </summary>
        /// <param name="exception">The exception to notice</param>
        /// <param name="parameters">Optional Parameters to display with the error</param>
        public static void NoticeError(Exception exception, IDictionary<string, string> parameters)
        {
            if (CanMonitor())
                Monitor.NoticeError(exception, parameters);
        }

        /// <summary>
        ///  Notices an error
        /// </summary>
        /// <param name="message">An error message</param>
        /// <param name="parameters">Optional Parameters to display with the error</param>
        public static void NoticeError(string message, IDictionary<string, string> parameters)
        {
            if (CanMonitor())
                Monitor.NoticeError(message, parameters);
        }

        /// <summary>
        /// Records a value against a specific metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value to record against the metric.</param>
        public static void RecordMetric(string name, float value)
        {
            if (CanMonitor())
                Monitor.RecordMetric(name, value);
        }

        /// <summary>
        /// Records a value against a specific metric.
        /// </summary>
        /// <param name="value">The value to record against the metric.</param>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        public static void RecordMetric(float value, params string[] orderedSegments)
        {
            if (CanMonitor())
                Monitor.RecordMetric(CreateName(orderedSegments), value);
        }

        /// <summary>
        /// Records a response time metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="duration">The response time metric.</param>
        public static void RecordResponseTimeMetric(string name, TimeSpan duration)
        {
            if (CanMonitor())
                Monitor.RecordResponseTimeMetric(name, duration);
        }

        /// <summary>
        /// Records a response time metric.
        /// </summary>
        /// <param name="duration">The response time metric.</param>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        public static void RecordResponseTimeMetric(TimeSpan duration, params string[] orderedSegments)
        {
            if (CanMonitor())
                Monitor.RecordResponseTimeMetric(CreateName(orderedSegments), duration);
        }

        /// <summary>
        /// Adds a custom property (name/value pair) to the monitoring transaction context.
        /// </summary>
        /// <param name="name">The name of the name/value pair to add to the monitoring transaction context.</param>
        /// <param name="value">The value of the name/value pair to add to the monitoring transaction context.</param>
        public static void AddCustomProperty(string name, string value)
        {
            if (SupportsMonitorProperties())
                MonitorProperties.AddCustomProperty(name, value);
        }

        /// <summary>
        /// Sets the name of the transaction.
        /// </summary>
        /// <param name="category">The category of this transaction, which you can use to distinguish different types of transactions.</param>
        /// <param name="name">The name of the transaction.</param>
        public static void SetTransactionName(string category, string name)
        {
            if (SupportsMonitorProperties())
                MonitorProperties.SetTransactionName(category, name);
        }

        /// <summary>
        /// Sets the user properties to the monitoring transaction context.
        /// </summary>
        /// <param name="userValue">Specify a name or user name to associate with this request. This value is assigned to the user key.</param>
        /// <param name="accountValue">Specify the name of a user account to associate with this request. This value is assigned to the account key.</param>
        /// <param name="productValue">Specify the name of a product to associate with this request. This value is assigned to the product key.</param>
        public static void SetUserProperties(string userValue, string accountValue, string productValue)
        {
            if (SupportsMonitorProperties())
                MonitorProperties.SetUserProperties(userValue, accountValue, productValue);
        }

        /// <summary>
        /// Records a custom event.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="payload">The payload/attributes of the event.</param>
        public static void RecordCustomEvent(string name, MonitorEvent payload)
        {
            if (CanMonitor())
                Monitor.RecordCustomEvent(name, payload);
        }

        private static bool CanMonitor()
        {
            if (Monitor != null)
            {
                return true;
            }

            if (MonitorFactory != null)
            {
                Monitor = MonitorFactory();
            }

            if (Monitor == null)
            {
                return false;
            }

            Monitor.AreaName = _areaName;
            MonitorProperties = Monitor as IMonitorProperties;
            return true;
        }

        /// <summary>
        /// Resets the internally managed Monitor built from the externally accessible MonitorFactory.
        ///
        /// NOTE: the current use case for this is to allow for reliable unit testing
        /// </summary>
        public static void ResetMonitor()
        {
            Monitor = null;
            MonitorProperties = null;
        }

        /// <summary>
        /// Indicates if the configured Monitor supports <see cref="IMonitorProperties"/>.
        /// </summary>
        /// <returns>True - if the Monitor supports <see cref="IMonitorProperties"/>; otherwise False.</returns>
        private static bool SupportsMonitorProperties()
        {
            return CanMonitor() ? MonitorProperties != null : false;
        }
    }
}
