using System;

namespace Alkami.Utilities.Certificates.Logging
{
    internal interface ILoggingAdapter
    {
        void LogTrace(string message);
        void LogDebug(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogCritical(string message);
        void LogTrace(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogCritical(string message, params object[] args);
    }
}
