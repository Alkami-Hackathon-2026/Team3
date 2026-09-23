// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="MonitoringLogger.cs" company="Alkami Technology, Inc.">
// //   Copyright 2013 Alkami Technology, Inc.  All rights reserved.
// // </copyright>
// // <summary>
// //  This class is a thin wrapper to provide automatic metrics collection when a log is generated at the ERROR or
// //  higher logging level
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Common.Logging;

namespace Alkami.Monitoring
{
    /// <summary>
    /// This class acts as a wrapper to bind automatic metric publishing (Metric.NoticeError) when an error is logged
    /// via the ILog.Error standard logging method.
    /// </summary>
    public static class MonitoringLoggingFactory
    {
        /// <summary>
        /// A factory method to create a wrapper to the ILog interface that ties all errors to metric
        /// publishing for internal tracking of errors.
        /// </summary>
        /// <param name="logger">The underlying logging implementation instance that will be wrapped</param>
        /// <returns>A wrapper instance that respects the ILog interface</returns>
        public static ILog WrapAsLoggingMonitor(ILog logger)
        {
            return new MonitoringLogger(logger);
        }

        private class MonitoringLogger : ILog
        {
            private readonly ILog _logger;

            public MonitoringLogger(ILog logger)
            {
                _logger = logger;
            }

            public void Trace(object message)
            {
                _logger.Trace(message);
            }

            public void Trace(object message, Exception exception)
            {
                _logger.Trace(message, exception);
            }

            public void TraceFormat(string format, params object[] args)
            {
                _logger.TraceFormat(format, args);
            }

            public void TraceFormat(string format, Exception exception, params object[] args)
            {
                _logger.TraceFormat(format, exception, args);
            }

            public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.TraceFormat(formatProvider, format, args);
            }

            public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.TraceFormat(formatProvider, format, exception, args);
            }

            public void Trace(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Trace(formatMessageCallback);
            }

            public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Trace(formatMessageCallback, exception);
            }

            public void Trace(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Trace(formatProvider, formatMessageCallback);
            }

            public void Trace(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Trace(formatProvider, formatMessageCallback, exception);
            }

            public void Debug(object message)
            {
                _logger.Debug(message);
            }

            public void Debug(object message, Exception exception)
            {
                _logger.Debug(message, exception);
            }

            public void DebugFormat(string format, params object[] args)
            {
                _logger.DebugFormat(format, args);
            }

            public void DebugFormat(string format, Exception exception, params object[] args)
            {
                _logger.DebugFormat(format, exception, args);
            }

            public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.DebugFormat(formatProvider, format, args);
            }

            public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.DebugFormat(formatProvider, format, exception, args);
            }

            public void Debug(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Debug(formatMessageCallback);
            }

            public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Debug(formatMessageCallback, exception);
            }

            public void Debug(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Debug(formatProvider, formatMessageCallback);
            }

            public void Debug(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Debug(formatProvider, formatMessageCallback, exception);
            }

            public void Info(object message)
            {
                _logger.Info(message);
            }

            public void Info(object message, Exception exception)
            {
                _logger.Info(message, exception);
            }

            public void InfoFormat(string format, params object[] args)
            {
                _logger.InfoFormat(format, args);
            }

            public void InfoFormat(string format, Exception exception, params object[] args)
            {
                _logger.InfoFormat(format, exception, args);
            }

            public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.InfoFormat(formatProvider, format, args);
            }

            public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.InfoFormat(formatProvider, format, exception, args);
            }

            public void Info(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Info(formatMessageCallback);
            }

            public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Info(formatMessageCallback, exception);
            }

            public void Info(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Info(formatProvider, formatMessageCallback);
            }

            public void Info(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Info(formatProvider, formatMessageCallback, exception);
            }

            public void Warn(object message)
            {
                _logger.Warn(message);
            }

            public void Warn(object message, Exception exception)
            {
                _logger.Warn(message, exception);
            }

            public void WarnFormat(string format, params object[] args)
            {
                _logger.WarnFormat(format, args);
            }

            public void WarnFormat(string format, Exception exception, params object[] args)
            {
                _logger.WarnFormat(format, exception, args);
            }

            public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.WarnFormat(formatProvider, format, args);
            }

            public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.WarnFormat(formatProvider, format, exception, args);
            }

            public void Warn(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Warn(formatMessageCallback);
            }

            public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Warn(formatMessageCallback, exception);
            }

            public void Warn(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Warn(formatProvider, formatMessageCallback);
            }

            public void Warn(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Warn(formatProvider, formatMessageCallback, exception);
            }

            public void Error(object message)
            {
                _logger.Error(message);
                Metric.NoticeError(message.ToString(), null);
            }

            public void Error(object message, Exception exception)
            {
                _logger.Error(message, exception);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string> {{"message", message.ToString()}});
            }

            public void ErrorFormat(string format, params object[] args)
            {
                _logger.ErrorFormat(format, args);
                Metric.NoticeError(string.Format(format, args), null);
            }

            public void ErrorFormat(string format, Exception exception, params object[] args)
            {
                _logger.ErrorFormat(format, exception, args);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string> {{"message", string.Format(format, args)}});
            }

            public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.ErrorFormat(formatProvider, format, args);
                Metric.NoticeError(string.Format(formatProvider, format, args), null);
            }

            public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.ErrorFormat(formatProvider, format, exception, args);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string>
                                       {{"message", string.Format(formatProvider, format, args)}});
            }

            public void Error(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Error(formatMessageCallback);
                // NO-OP: callbacks are not supported for the Metric monitor
            }

            public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Error(formatMessageCallback, exception);
                Metric.NoticeError(exception);
            }

            public void Error(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Error(formatProvider, formatMessageCallback);
                // NO-OP: callbacks are not supported for the Metric monitor
            }

            public void Error(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Error(formatProvider, formatMessageCallback, exception);
                Metric.NoticeError(exception);
            }

            public void Fatal(object message)
            {
                _logger.Fatal(message);
                Metric.NoticeError(message.ToString(), null);
            }

            public void Fatal(object message, Exception exception)
            {
                _logger.Fatal(message, exception);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string> {{"message", message.ToString()}});
            }

            public void FatalFormat(string format, params object[] args)
            {
                _logger.FatalFormat(format, args);
                Metric.NoticeError(string.Format(format, args), null);
            }

            public void FatalFormat(string format, Exception exception, params object[] args)
            {
                _logger.FatalFormat(format, exception, args);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string> {{"message", string.Format(format, args)}});
            }

            public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
                _logger.FatalFormat(formatProvider, format, args);
                Metric.NoticeError(string.Format(formatProvider, format, args), null);
            }

            public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception,
                params object[] args)
            {
                _logger.FatalFormat(formatProvider, format, exception, args);
                Metric.NoticeError(exception,
                                   new Dictionary<string, string>
                                       {{"message", string.Format(formatProvider, format, args)}});
            }

            public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Fatal(formatMessageCallback);
                // NO-OP: callbacks are not supported for the Metric monitor
            }

            public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
                _logger.Fatal(formatMessageCallback, exception);
                Metric.NoticeError(exception);
            }

            public void Fatal(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback)
            {
                _logger.Fatal(formatProvider, formatMessageCallback);
                // NO-OP: callbacks are not supported for the Metric monitor
            }

            public void Fatal(IFormatProvider formatProvider,
                Action<FormatMessageHandler> formatMessageCallback,
                Exception exception)
            {
                _logger.Fatal(formatProvider, formatMessageCallback, exception);
                Metric.NoticeError(exception);
            }

            public bool IsTraceEnabled => _logger.IsTraceEnabled;
            public bool IsDebugEnabled => _logger.IsDebugEnabled;
            public bool IsErrorEnabled => _logger.IsErrorEnabled;
            public bool IsFatalEnabled => _logger.IsFatalEnabled;
            public bool IsInfoEnabled => _logger.IsInfoEnabled;
            public bool IsWarnEnabled => _logger.IsWarnEnabled;
            public IVariablesContext GlobalVariablesContext => _logger.GlobalVariablesContext;
            public IVariablesContext ThreadVariablesContext => _logger.ThreadVariablesContext;
            public INestedVariablesContext NestedThreadVariablesContext => _logger.NestedThreadVariablesContext;
        }
    }
}