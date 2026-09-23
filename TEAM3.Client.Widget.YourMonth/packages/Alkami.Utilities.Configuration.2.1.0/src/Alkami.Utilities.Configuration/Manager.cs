using Common.Logging;
#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// Manager for getting Settings from AppSettings, Environment Variables, and IConfiguration
    /// </summary>
    public static class Manager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Manager));

        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
#pragma warning disable CS0067 // The event 'Manager.ConfigurationsChanged' is never used
        public static event EventHandler<CollectionChangeEventArgs> ConfigurationsChanged;
#pragma warning restore CS0067 // The event 'Manager.ConfigurationsChanged' is never used

        internal static readonly List<SettingsBase> Settings = new List<SettingsBase>()
           {
               new EnvironmentVariables(),
               new ConfigManager()
           };

        /// <summary>
        /// Register another <paramref name="customSettingsBase"/> to lookup settings
        /// </summary>
        /// <param name="customSettingsBase"></param>
        public static void Register(SettingsBase customSettingsBase)
        {
            Settings.Add(customSettingsBase);
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// Sets up so the Settings will pull from the IConfiguration before pulling from Environment Variables or AppSettings
        /// </summary>
        /// <param name="configuration"></param>
        public static void AddConfigurationSettingManager(IConfiguration configuration)
        {
            AddConfigurationSettingManager(configuration, true);
        }

        /// <summary>
        /// Sets up so the Settings will pull from the IConfiguration
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="overridesOtherSettings">If <c>true</c> will choose these settings over the Environment Variables or AppSettings</param>
        public static void AddConfigurationSettingManager(IConfiguration configuration, bool overridesOtherSettings)
        {
            if (Settings.Any(x => x is MicrosoftConfigurationManager))
            {
                return;
            }

            var manager = new MicrosoftConfigurationManager(configuration);

            if (overridesOtherSettings)
            {
                Settings.Insert(0, manager);
            }
            else
            {
                Settings.Add(manager);
            }
        }

#endif

        /// <summary>
        /// Gets a settings from the registered <see cref="SettingsBase"/> classes
        /// </summary>
        /// <param name="name">Setting name to look up</param>
        /// <returns></returns>
        public static string GetSetting(string name)
        {
#if NET6_0_OR_GREATER
            name = name switch
            {
                "Environment.Name" => "ALKAMI_ENVIRONMENT_FULLNAME",
                "Environment.NameSafeDesignation" => "ALKAMI_ENVIRONMENT_NAME",
                "Environment.Type" => "ALKAMI_ENVIRONMENT_TYPE",
                "SubscriptionServiceMachine" => "ALKAMI_SUBSCRIPTION_SERVICE_MACHINE",
                "consumer.localstack.sqsurl" => "ALKAMI_CONSUMER_LOCALSTACK_SQS_URL",
                "publisher.localstack.sqsurl" => "ALKAMI_PUBLISHER_LOCALSTACK_SQS_URL",
                _ => name
            };
#endif

            foreach (var setting in Settings)
            {
                var value = setting.Get(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Logger.Trace(t => t($"Key {name} found in {setting.GetType().Name}"));
                    return value;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the list of all keys that are available to get settings for
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeys()
        {
            return Settings.SelectMany(x => x.AllKeys()).Distinct().ToArray();
        }
    }
}