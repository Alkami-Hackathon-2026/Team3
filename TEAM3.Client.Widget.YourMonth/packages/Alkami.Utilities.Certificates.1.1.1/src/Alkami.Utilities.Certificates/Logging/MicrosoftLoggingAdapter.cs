#if NET6_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Alkami.Utilities.Certificates.Logging
{
    [ExcludeFromCodeCoverage]
    internal class MicrosoftLoggingAdapter : ILoggingAdapter
    {
        private readonly ILogger _logger;
        
        internal MicrosoftLoggingAdapter(ILogger logger)
        {
            _logger = logger;
        }
        
        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogCritical(string message)
        {
            _logger.LogCritical(message);
        }

        public void LogTrace(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogCritical(string message, params object[] args)
        {
            _logger.LogCritical(message, args);
        }
    }
}
#endif
