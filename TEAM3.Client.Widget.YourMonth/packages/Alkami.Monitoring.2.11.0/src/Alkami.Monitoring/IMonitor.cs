using System;
using System.Collections.Generic;

namespace Alkami.Monitoring
{
    /// <summary>
    /// The IMonitor interface provides an abstraction for recording performance metrics for monitoring the health of an application.
    /// </summary>
    public interface IMonitor
    {
        /// <summary>
        /// The name of the area for metrics collection
        /// </summary>
        string AreaName { get; set; }

        /// <summary>
        /// Creates a consistent metric name for the metrics collection tool.
        /// </summary>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        /// <returns>The complete name for the metric.</returns>
        string CreateName(params string[] orderedSegments);

        /// <summary>
        /// Increments a metric counter.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        void IncrementCounter(string name);

        /// <summary>
        /// Increments a metric counter.
        /// </summary>
        /// <param name="orderedSegments">The name of the metric.</param>
        void IncrementCounter(params string[] orderedSegments);

        /// <summary>
        /// Notices an error
        /// </summary>
        /// <param name="exception">The exception to notice</param>
        void NoticeError(Exception exception);

        /// <summary>
        ///  Notices an error
        /// </summary>
        /// <param name="exception">The exception to notice</param>
        /// <param name="parameters">Optional Parameters to display with the error</param>
        void NoticeError(Exception exception, IDictionary<string, string> parameters);

        /// <summary>
        ///  Notices an error
        /// </summary>
        /// <param name="message">An error message</param>
        /// <param name="parameters">Optional Parameters to display with the error</param>
        void NoticeError(string message, IDictionary<string, string> parameters);

        /// <summary>
        /// Records a value against a specific metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value to record against the metric.</param>
        void RecordMetric(string name, float value);

        /// <summary>
        /// Records a value against a specific metric.
        /// </summary>
        /// <param name="value">The value to record against the metric.</param>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        void RecordMetric(float value, params string[] orderedSegments);

        /// <summary>
        /// Records a response time metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="duration">The response time metric.</param>
        void RecordResponseTimeMetric(string name, TimeSpan duration);

        /// <summary>
        /// Records a response time metric.
        /// </summary>
        /// <param name="duration">The response time metric.</param>
        /// <param name="orderedSegments">The collection of ordered segments for the metric name.</param>
        void RecordResponseTimeMetric(TimeSpan duration, params string[] orderedSegments);

        /// <summary>
        /// Records a custom event.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="payload">The payload/attributes of the event.</param>
        void RecordCustomEvent(string name, MonitorEvent payload);
    }
}
