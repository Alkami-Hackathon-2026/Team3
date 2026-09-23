using System;
using Common.Logging;

namespace Alkami.Utilities.Certificates.Logging
{
    internal class CommonLoggingAdapter : ILoggingAdapter
    {
        private readonly ILog _logger;

        public CommonLoggingAdapter(ILog logger)
        {
            _logger = logger;
        }

        public void LogTrace(string message)
       {
            _logger.Trace(t => t(message));
        }

        public void LogDebug(string message)
        {
            _logger.Debug(d => d(message));
        }

        public void LogWarning(string message)
        {
            _logger.Warn(w => w(message));
        }

        public void LogError(string message)
        {
            _logger.Error(e => e(message));
        }

        public void LogCritical(string message)
        {
            this.LogError(message);
        }

        public void LogTrace(string message, params object[] args)
        {
            _logger.TraceFormat(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.DebugFormat(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.WarnFormat(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.ErrorFormat(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.ErrorFormat(message, exception, args);
        }

        public void LogCritical(string message, params object[] args)
        {
            _logger.FatalFormat(message, args);
        }
    }
}