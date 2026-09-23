using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Alkami.Monitoring;
using Alkami.Utilities.Configuration;
using Alkami.Utilities.Cryptography;
using Alkami.Utilities.Extensions;
using Common.Logging;
using Newtonsoft.Json;

#if NETFRAMEWORK
using System.Linq;
using Microsoft.Web.Administration;
#endif

namespace Alkami.Utilities
{
    /// <summary>
    /// Static helper methods and utilities when logging
    /// </summary>
    public static class LogUtility
    {
        /// <summary>
        /// A standard column separator to be used when logging
        /// </summary>
        public const string COLUMN_SEPARATOR = "|*|";

        /// <summary>
        /// The name of the Correlation_ID property to correlate multiple log entries to a single request
        /// </summary>
        public const string CORRELATION_ID = "CorrelationID";

        /// <summary>
        /// The name of the BankIdentifier Property in the Log4Net ThreadContext Properties
        /// </summary>
        public const string BankIdentifier = "BankIdentifier";
        /// <summary>
        /// The name of the UserIdentifier Property in the Log4Net ThreadContext Properties
        /// </summary>
        public const string UserIdentifier = "UserIdentifier";
        /// <summary>
        /// The name of the UserSessionId Property in the Log4Net ThreadContext Properties
        /// </summary>
        public const string UserSessionId = "UserSessionId";
        /// <summary>
        /// The name of the Quartz JobName Property in the Log4Net ThreadContext Properties
        /// </summary>
        private const string QuartzJobName = "QuartzJobName";
        /// <summary>
        /// 
        /// </summary>
        private const string QuartzJobGroup = "QuartzJobGroup";

        #region Logging Injection Methods

/// <summary>
/// Injects the Current Claims
/// </summary>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectClaimsForLogging(Dictionary<string, string> claimsDictionary)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            foreach (var claim in claimsDictionary)
            {
                log4net.LogicalThreadContext.Properties[claim.Key] = claim.Value;
                Metric.AddCustomProperty(claim.Key, claim.Value);
            }
        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankUrlSignature">The banks URI</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Uri bankUrlSignature)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankUrlSignature;

            Metric.AddCustomProperty(BankIdentifier, bankUrlSignature.ToString());
        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankIdentifier">The banks Identifier</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Guid bankIdentifier)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankIdentifier;

            Metric.AddCustomProperty(BankIdentifier, bankIdentifier.ToString());
        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankIdentifier">The banks Identifier</param>
        /// <param name="stsUserId">The stsUserId or the UserName</param>
        /// <param name="userSessionId">The session id of the user</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Guid bankIdentifier, string stsUserId, string userSessionId)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");

            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankIdentifier;
            log4net.LogicalThreadContext.Properties[UserIdentifier] = stsUserId;
            log4net.LogicalThreadContext.Properties[UserSessionId] = userSessionId;

            Metric.AddCustomProperty(BankIdentifier, bankIdentifier.ToString());
            Metric.AddCustomProperty(UserIdentifier, stsUserId);
            Metric.AddCustomProperty(UserSessionId, userSessionId);
        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankUrlSignature">The Banks Uri</param>
        /// <param name="stsUserId">The STS ID of the user</param>
        /// <param name="userSessionId">The session id of the user</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Uri bankUrlSignature, string stsUserId, string userSessionId)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankUrlSignature;
            log4net.LogicalThreadContext.Properties[UserIdentifier] = stsUserId;
            log4net.LogicalThreadContext.Properties[UserSessionId] = userSessionId;

            Metric.AddCustomProperty(BankIdentifier, bankUrlSignature.ToString());
            Metric.AddCustomProperty(UserIdentifier, stsUserId);
            Metric.AddCustomProperty(UserSessionId, userSessionId);
        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankUrlSignature">The Banks Uri</param>
        /// <param name="userIdentifier">The userIdentifier of the user</param>
        /// <param name="userSessionId">The session id of the user</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Uri bankUrlSignature, Guid userIdentifier, string userSessionId)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankUrlSignature;
            log4net.LogicalThreadContext.Properties[UserIdentifier] = userIdentifier;
            log4net.LogicalThreadContext.Properties[UserSessionId] = userSessionId;

            Metric.AddCustomProperty(BankIdentifier, bankUrlSignature.ToString());
            Metric.AddCustomProperty(UserIdentifier, userIdentifier.ToString());
            Metric.AddCustomProperty(UserSessionId, userSessionId);

        }

        /// <summary>
        ///  Injects some custom properties into Logging, for public services
        /// </summary>
        /// <param name="bankIdentifier">The Bank identifier</param>
        /// <param name="userIdentifier">The userIdentifier of the user</param>
        /// <param name="userSessionId">The session id of the user</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLoggingProperties(Guid bankIdentifier, Guid userIdentifier, string userSessionId)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            log4net.LogicalThreadContext.Properties[BankIdentifier] = bankIdentifier;
            log4net.LogicalThreadContext.Properties[UserIdentifier] = userIdentifier;
            log4net.LogicalThreadContext.Properties[UserSessionId] = userSessionId;

            Metric.AddCustomProperty(BankIdentifier, bankIdentifier.ToString());
            Metric.AddCustomProperty(UserIdentifier, userIdentifier.ToString());
            Metric.AddCustomProperty(UserSessionId, userSessionId);
        }

        /// <summary>
        /// Injects the Trigger group and Name into Log4Net ThreadContext
        /// </summary>
        /// <param name="triggerName">The TriggerName</param>
        /// <param name="triggerGroup">The Trigger group</param>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectLog4NetQuartzProperties(string triggerName, string triggerGroup)
        {
            log4net.LogicalThreadContext.Properties[QuartzJobName] = triggerName;
            log4net.LogicalThreadContext.Properties[QuartzJobGroup] = triggerGroup;
        }

        /// <summary>
        /// Clear all the properties
        /// </summary>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void ClearLoggingProperties()
        {
            log4net.LogicalThreadContext.Properties.Clear();
        }

        #endregion

        #region Application Startup Logging

#if NETFRAMEWORK

        /// <summary>
        /// Logs information about the loaded assemblies during application startup
        /// </summary>
        /// <param name="logger">The Logger</param>
        /// <param name="executingAssembly">The Executing Assembly</param>
        public static void LogIISApplicationStartup(ILog logger, Assembly executingAssembly)
        {
            if (!logger.IsDebugEnabled)
            {
                return;
            }

            try
            {
                LogCommonValues(logger, executingAssembly);

                var siteName = System.Web.Hosting.HostingEnvironment.SiteName;

                var site =
                    WebConfigurationManager.GetSection(null, null, "system.applicationHost/sites")
                             .GetCollection()
                             .FirstOrDefault(x => string.Compare(x["name"].ToString(), siteName, StringComparison.OrdinalIgnoreCase) == 0);

                if (site != null)
                {
                    site.GetCollection("bindings").ForEach(x => logger.DebugFormat("Site is using binding {0}, BindingInformation {1}", x["protocol"], x["bindingInformation"]));
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("An Exception was thrown when trying to log application startup", ex);
            }
        }

#endif

        private static void LogCommonValues(ILog logger, Assembly executingAssembly)
        {
            executingAssembly.GetReferencedAssemblies().ForEach(x => logger.DebugFormat("Referenced Assembly Name '{0}' '{1}'", x.Name, x.Version));

            AppDomain.CurrentDomain.GetAssemblies().ForEach(x => logger.DebugFormat("Loaded Assembly Name '{0}' Version '{1}'", x.FullName, x.GetName().Version));

            logger.DebugFormat("Process is currently running under {0} {1}", Environment.UserDomainName, Environment.UserName);

            var process = System.Diagnostics.Process.GetCurrentProcess();
            logger.DebugFormat("Process Name '{0}', PID '{1}'", process.ProcessName, process.Id);
        }

        /// <summary>
        /// Logs information about the loaded assemblies during application startup
        /// </summary>
        /// <param name="logger">The Logger</param>
        /// <param name="executingAssembly">The Executing Assembly</param>
        public static void LogWindowsServiceApplicationStartup(ILog logger, Assembly executingAssembly)
        {
            if (!logger.IsDebugEnabled)
            {
                return;
            }

            try
            {
                LogCommonValues(logger, executingAssembly);
            }
            catch (Exception ex)
            {

                logger.DebugFormat("An Exception was thrown when trying to log application startup {0}", ex);
            }
        }
        #endregion

        /// <summary>
        /// Trace-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void TraceCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsTraceEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.TraceFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Info-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void InfoCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsInfoEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.InfoFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Warn-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void WarnCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsWarnEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.WarnFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Error-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void ErrorCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsErrorEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.ErrorFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Debug-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void DebugCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsDebugEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.DebugFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Fatal-logs the current call stack with the specified message.
        /// </summary>
        /// <param name="logger">Logger to use for logging.</param>
        /// <param name="logMessage">Log message to add.</param>
        public static void FatalCallStack(this ILog logger, string logMessage)
        {
            if (logger.IsFatalEnabled && ApplicationConfiguration.Logging.LogCallStack)
            {
                // This is here for easy debugging - StringBuilder doesn't
                // have an Inspect icon in the debugger but String does.
                string traceValue = BuildCallStack();

                logger.FatalFormat("{0}, stack trace:\n{1}", logMessage, traceValue);
            }
        }

        /// <summary>
        /// Logs the supplied message if Trace is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void LogTrace(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(message);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Trace is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">An array of objects that contain zero or more objects to format.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogTraceEncrypted(this ILog logger, string message, params object[] args)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(EncryptMessage(string.Format(message, args)));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Trace is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogTraceEncrypted(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsTraceEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Trace(EncryptMessage(callback.ToString()));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message and exception if Trace is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogTraceEncrypted(this ILog logger, Action<FormatMessageHandler> message, Exception exception)
        {
            if (logger.IsTraceEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Trace(EncryptMessage(callback.ToString()), exception);
            }
        }

        /// <summary>
        /// Logs the supplied message if Debug is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void LogDebug(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Debug is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">An array of objects that contain zero or more objects to format.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogDebugEncrypted(this ILog logger, string message, params object[] args)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(EncryptMessage(string.Format(message, args)));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Debug is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogDebugEncrypted(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsDebugEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Debug(EncryptMessage(callback.ToString()));
            }
        }

        /// <summary>
        /// Logs the supplied message if Debug is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public static void LogDebug(this ILog logger, Action<FormatMessageHandler> message, Exception ex)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message, ex);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message and exception if Debug is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogDebugEncrypted(this ILog logger, Action<FormatMessageHandler> message, Exception exception)
        {
            if (logger.IsDebugEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Debug(EncryptMessage(callback.ToString()), exception);
            }
        }

        /// <summary>
        /// Logs the supplied message if Info is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void LogInfo(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(message);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Info is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">An array of objects that contain zero or more objects to format.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogInfoEncrypted(this ILog logger, string message, params object[] args)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(EncryptMessage(string.Format(message, args)));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Info is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogInfoEncrypted(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsInfoEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Info(EncryptMessage(callback.ToString()));
            }
        }

        /// <summary>
        /// Logs the supplied message if Warn is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void LogWarn(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsWarnEnabled)
            {
                logger.Warn(message);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Warn is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">An array of objects that contain zero or more objects to format.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogWarnEncrypted(this ILog logger, string message, params object[] args)
        {
            if (logger.IsWarnEnabled)
            {
                logger.Warn(EncryptMessage(string.Format(message, args)));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Warn is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogWarnEncrypted(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsWarnEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Warn(EncryptMessage(callback.ToString()));
            }
        }

        /// <summary>
        /// Error Logs the supplied message if Error is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void LogError(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(message);
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Error is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">An array of objects that contain zero or more objects to format.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogErrorEncrypted(this ILog logger, string message, params object[] args)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(EncryptMessage(string.Format(message, args)));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message if Error is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
#if !NETFRAMEWORK
        [Obsolete("Use the Alkami.Extensions.EncryptedLogging library, see https://confluence.alkami.com/x/JMgcBg")]
#endif
        public static void LogErrorEncrypted(this ILog logger, Action<FormatMessageHandler> message)
        {
            if (logger.IsErrorEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Error(EncryptMessage(callback.ToString()));
            }
        }

        /// <summary>
        /// Encrypts and logs the supplied message and exception if Error is Enabled.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void LogErrorEncrypted(this ILog logger, Action<FormatMessageHandler> message, Exception exception)
        {
            if (logger.IsErrorEnabled)
            {
                var callback = new FormatMessageCallbackFormattedMessage(message);
                logger.Error(EncryptMessage(callback.ToString()), exception);
            }
        }

        /// <summary>
        /// Gets the CorrelationID
        /// </summary>
        /// <returns>A Guid which is the CorrlationID. Will be an emtpy Guid if one is not created.</returns>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static Guid GetCorrelationId()
        {
            var retVal = Guid.Empty;

            if (log4net.LogicalThreadContext.Properties[CORRELATION_ID] != null)
                Guid.TryParse(log4net.LogicalThreadContext.Properties[CORRELATION_ID].ToString(), out retVal);

            return retVal;
        }

        /// <summary>
        /// Gets the BankIdentifier
        /// </summary>
        /// <returns>A Guid which is the BankIdentifier. Will be an emtpy Guid if one is not created.</returns>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void SetThreadContextProperties(Dictionary<string, object> properties)
        {
            foreach (var p in properties)
            {
                log4net.LogicalThreadContext.Properties[p.Key] = p.Value;
            }
        }

        /// <summary>
        /// Gets the BankIdentifier
        /// </summary>
        /// <returns>A Guid which is the BankIdentifier. Will be an emtpy Guid if one is not created.</returns>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static Dictionary<string, object> GetThreadContextProperties()
        {
            var properties = new Dictionary<string, object>();
            if (log4net.LogicalThreadContext.Properties[CORRELATION_ID] != null)
                properties.Add(CORRELATION_ID, log4net.LogicalThreadContext.Properties[CORRELATION_ID]);
            if (log4net.LogicalThreadContext.Properties[BankIdentifier] != null)
                properties.Add(BankIdentifier, log4net.LogicalThreadContext.Properties[BankIdentifier]);
            if (log4net.LogicalThreadContext.Properties[UserIdentifier] != null)
                properties.Add(UserIdentifier, log4net.LogicalThreadContext.Properties[UserIdentifier]);
            if (log4net.LogicalThreadContext.Properties[QuartzJobName] != null)
                properties.Add(QuartzJobName, log4net.LogicalThreadContext.Properties[QuartzJobName]);
            if (log4net.LogicalThreadContext.Properties[QuartzJobGroup] != null)
                properties.Add(QuartzJobGroup, log4net.LogicalThreadContext.Properties[QuartzJobGroup]);
            return properties;
        }

        /// <summary>
        /// Injects the CorrelationID. Will only do so if the correlationID is not already set, to prevent it from being overwritten.
        /// </summary>
#if !NETFRAMEWORK
        [Obsolete("Do not use since Log4Net is no longer used")]
#endif
        public static void InjectCorrelationID(Guid guid)
        {
            using var _ = Metric.BeginSuppression("OpenTelemetry");
            if (GetCorrelationId() == Guid.Empty)
            {
                log4net.LogicalThreadContext.Properties[CORRELATION_ID] = guid;
                Metric.AddCustomProperty(CORRELATION_ID, guid.ToString());
            }
        }
        /// <summary>
        /// Creates a Log entry that contains the Request made to the core
        /// </summary>
        /// <param name="coreMethodName">The Name of the Method on the Core we are calling</param>
        /// <param name="requestString">The actual Request</param>
        /// <returns>A Properly formatted string</returns>
        public static string CreateCoreRequestEntry(string coreMethodName, string requestString)
        {
            return CreateCoreEntry("CREQ", coreMethodName, "XML", requestString);
        }

        /// <summary>
        /// Creates a Log entry that contains the Response received from core
        /// </summary>
        /// <param name="coreMethodName">The Name of the Method on the Core we are calling</param>
        /// <param name="requestString">The actual Request</param>
        /// <returns>A Properly formatted string</returns>
        public static string CreateCoreResponseEntry(string coreMethodName, string requestString)
        {
            return CreateCoreEntry("CRES", coreMethodName, "XML", requestString);
        }

        private static string CreateCoreEntry(string entryprefix, string coreMethodName, string serialization, string requestString)
        {
            return string.Format("{0} {1} CoreRequestMethod {2} {1} {3} {4}", entryprefix, COLUMN_SEPARATOR, coreMethodName,
                                 serialization, EncryptMessage(requestString));
        }

        /// <summary>
        /// Creates a log entry that conains the Json-serialized Request sent to core 
        /// </summary>
        /// <param name="coreMethod">The Name of the Method on the Core we are calling</param>
        /// <param name="objectToLog">The actual Request object</param>
        /// <param name="indent">optional boolean indicating indented Json format</param>
        /// <returns>A properly formatted string</returns>
        public static string CreateCoreRequestJsonEntry(string coreMethod, object objectToLog, bool indent = false)
        {
            return CreateCoreEntry("CREQ", coreMethod, "JSON", ToJson(objectToLog, indent));
        }

        /// <summary>
        /// Creates a log entry that conains the Json-serialized Response received from core 
        /// </summary>
        /// <param name="coreMethod">The Name of the Method on the Core we are calling</param>
        /// <param name="objectToLog">The actual REsponse object</param>
        /// <param name="indent">optional boolean indicating indented Json format</param>
        /// <returns>A properly formatted string</returns>
        public static string CreateCoreResponseJsonEntry(string coreMethod, object objectToLog, bool indent = false)
        {
            return CreateCoreEntry("CRES", coreMethod, "JSON", ToJson(objectToLog, indent));
        }

        /// <summary>
        /// Boolean for whether or not log encryption is enabled
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <returns>True if log encryption is enabled, false otherwise</returns>
        public static bool IsEncryptionEnabled(this ILog logger)
        {
            return ShouldEncryptMessage;
        }

        private static string ToJson(object objectToSerialize, bool indent)
        {
            if (objectToSerialize == null)
                return "[null]";

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var serializer = JsonSerializer.Create(settings);
            var sb = new StringBuilder();

            var sw = new StringWriter(sb);
            using (JsonWriter w = new JsonTextWriter(sw))
            {
                if (indent)
                    w.Formatting = Newtonsoft.Json.Formatting.Indented;

                serializer.Serialize(w, objectToSerialize);
            }
            return sb.ToString();
        }

        private static string BuildCallStack()
        {
            StringBuilder traceBuffer = new StringBuilder();
            StackTrace stackTrace = new StackTrace(true);
            StackFrame[] stackFrames = stackTrace.GetFrames();

            if (stackFrames != null)
            {
                for (int i = 0; i < stackFrames.Length; i++)
                {
                    traceBuffer
                        .Append("  Frame ")
                        .Append(i)
                        .Append(": ")
                        .Append(stackFrames[i].ToString());
                }
            }
            return (traceBuffer.ToString());
        }

        internal const int CharacterEncryptionLimit = 16379;

        /// <summary>
        /// Encrypt a message (this will use a X.509 certificate)
        /// </summary>
        /// <remarks>
        /// Strings with more than 16379 characters that contain characters such as combining diacritical marks or emoji (which appear to be one character but are actually
        /// two characters) are not supported.
        /// </remarks>
        /// <param name="message">The message to encrypt</param>
        /// <returns></returns>
        public static string EncryptMessage(string message)
        {
            if (ShouldEncryptMessage)
            {
                try
                {
                    StringBuilder outputBuilder = new StringBuilder();
                    double forLoopLimit = message.Length / (double)CharacterEncryptionLimit;
                    for (int i = 0; i < forLoopLimit; i++)
                    {
                        string slice = message.Substring(i * CharacterEncryptionLimit, Math.Min(CharacterEncryptionLimit, message.Length - i * CharacterEncryptionLimit));
                        outputBuilder.Append($"[ENCRYPT({AsymmetricEncryption.EncryptWithThumbprint(slice, EncryptionCertificate)})]");
                    }
                    return outputBuilder.ToString();
                }
                catch (Exception ex)
                {
                    return $"[ENCRYPTERROR({ex})]";
                }
            }

            return message;
        }

        /// <summary>
        /// Takes an encrypted string, and gets the thumbrint of the certificate used to encrypt it originally
        /// </summary>
        /// <param name="payload">The encrypted string</param>
        /// <returns>The thumbrint of the certificate used to encrypt the payload string</returns>
        public static string ReadThumprint(string payload)
        {
            byte[] byteArray = Convert.FromBase64String(payload);
            MemoryStream memStream = new MemoryStream(byteArray);
            var tempByteArr = ReadBytesFromStream(memStream);

            return System.Text.Encoding.UTF8.GetString(tempByteArr);
        }

        /// <summary>
        /// helper function for ReadThumbprint
        /// finds the length of the encrypted text
        /// (which is written in the first 2 bytes of stream)
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The length which was written in the stream</returns>
        internal static int ReadLengthFromStream(Stream stream)
        {
            using (var sr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                return sr.ReadUInt16();
            }
        }

        /// <summary>
        /// helper function for ReadThumbprint
        /// converts a stream in to byte array
        /// </summary>
        /// <param name="stream">The stream to read from and convert</param>
        /// <param name="bytesToRead">the number of bytes to read</param>
        /// <returns></returns>
        internal static byte[] ReadBytesFromStream(Stream stream, int bytesToRead = 0)
        {
            // If bytesToRead is 0, read the length of bytes from the stream
            if (bytesToRead == 0)
                bytesToRead = ReadLengthFromStream(stream);

            byte[] buffer = new byte[bytesToRead];

            int totalRead = 0;
            while (totalRead < bytesToRead)
            {
                int bytesRead = stream.Read(buffer, totalRead, bytesToRead - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            return buffer;
        }

        private static bool ShouldEncryptMessage
        {
            get
            {
                return ApplicationConfiguration.Logging.Encryption.EncryptionEnabled;
            }
        }

        private static X509Certificate2 encryptionCertificate;
        internal static X509Certificate2 EncryptionCertificate
        {
            get
            {
                return encryptionCertificate ?? (encryptionCertificate =
                           CertificateUtility.GetCertificateIfPresent(ApplicationConfiguration.Logging.Encryption.StoreName,
                                                                      ApplicationConfiguration.Logging.Encryption.StoreLocation,
                                                                      ApplicationConfiguration.Logging.Encryption.FindType,
                                                                      ApplicationConfiguration.Logging.Encryption.FindValue));
            }
            set
            {
                encryptionCertificate = value;
            }
        }
    }

#region EncryptMessageCallbackFormattedMessage

    /// <summary>
    /// Format message on demand.
    /// </summary>
    internal class FormatMessageCallbackFormattedMessage
    {
        /// <summary>
        /// The cached message
        /// </summary>
        private volatile string cachedMessage;

        /// <summary>
        /// The format provider
        /// </summary>
        private readonly IFormatProvider formatProvider;

        /// <summary>
        /// The format message callback
        /// </summary>
        private readonly System.Action<FormatMessageHandler> formatMessageCallback;

        /// <summary>
        /// The cached format
        /// </summary>
        private volatile string cachedFormat;

        /// <summary>
        /// The cached arguments
        /// </summary>
        private volatile object[] cachedArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatMessageCallbackFormattedMessage"/> class.
        /// </summary>
        /// <param name="formatMessageCallback">The format message callback.</param>
        internal FormatMessageCallbackFormattedMessage(System.Action<FormatMessageHandler> formatMessageCallback)
        {
            this.formatMessageCallback = formatMessageCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatMessageCallbackFormattedMessage"/> class.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="formatMessageCallback">The format message callback.</param>
        internal FormatMessageCallbackFormattedMessage(IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
        {
            this.formatProvider = formatProvider;
            this.formatMessageCallback = formatMessageCallback;
        }

        /// <summary>
        /// Calls <see cref="formatMessageCallback"/> and returns result.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (cachedMessage == null && formatMessageCallback != null)
            {
                formatMessageCallback(FormatMessage);
            }
            return cachedMessage;
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        private string FormatMessage(string format, params object[] args)
        {
            if (args.Length > 0 && formatProvider != null)
                cachedMessage = string.Format(formatProvider, format, args);
            else if (args.Length > 0)
                cachedMessage = string.Format(format, args);
            else if (formatProvider != null)
                cachedMessage = string.Format(formatProvider, format);
            else
                cachedMessage = format;

            cachedFormat = format;
            cachedArguments = args;

            return cachedMessage;
        }
    }

#endregion
}
