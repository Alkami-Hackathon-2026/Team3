#if NETFRAMEWORK
using System.Configuration;
using Common.Logging;
using Topshelf;
using Topshelf.HostConfigurators;

namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The MicroserviceConfiguration class is a helper class to configure microservice settings and recovery
    /// </summary>
    public static class MicroserviceConfiguration
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MicroserviceConfiguration));
        /// <summary>
        /// Sets up the Topshelf ServiceRecovery Configuration
        /// </summary>
        public static void ConfigureServiceRecovery(HostConfigurator configurator)
        {
            var delayInMinutes = 1;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Service.RestartDelay"]))
                delayInMinutes = int.Parse(ConfigurationManager.AppSettings["Service.RestartDelay"]);
            else
                _logger.Debug($"Restart Delay not set in app.config. Using default of 1 minute");

            var resetPeriod = 1;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Service.ResetPeriod"]))
                resetPeriod = int.Parse(ConfigurationManager.AppSettings["Service.ResetPeriod"]);
            else
                _logger.Debug($"Reset Period not set in app.config. Using default of 1 day");


            configurator.EnableServiceRecovery(recoveryConfigurator =>
            {
                //will restart the service every minute after failure
                recoveryConfigurator.RestartService(delayInMinutes);
                //for crashed or non-zero exits
                recoveryConfigurator.OnCrashOnly();
                //number of days until the error count resets
                recoveryConfigurator.SetResetPeriod(resetPeriod);
            });
        }
    }

}
#endif
