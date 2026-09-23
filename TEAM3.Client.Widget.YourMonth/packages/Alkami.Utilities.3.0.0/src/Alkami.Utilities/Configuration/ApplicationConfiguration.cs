using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The ApplicationConfiguration class is a helper class to get the ORB application 
    /// configuration from the application configuration file.
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// An AppSettings delegate to use for unit testing configuration settings
        /// </summary>
        internal static Func<NameValueCollection> AppSettingsFunc = () => ConfigurationManager.AppSettings;

        private static ApplicationConfigurationSection applicationConfiguration;

        /// <summary>
        /// Returns the Application Name configured for this application.
        /// </summary>
        public static string Name
        {
            get { return Configuration.Name; }
        }

        /// <summary>
        /// Returns the Environment section configured for this application.
        /// </summary>
        public static ApplicationConfigurationSection.EnvironmentElement Environment
        {
            get { return Configuration.Environment; }
        }

        /// <summary>
        /// Returns the Application section configured for this application.
        /// </summary>
        public static ApplicationConfigurationSection.LoggingElement Logging
        {
            get { return Configuration.Logging; }
        }

        /// <summary>
        /// A static method to get the ORB Application Configuration from
        /// the application configuration file.
        /// </summary>
        private static ApplicationConfigurationSection Configuration
        {
            get
            {
                applicationConfiguration = (ApplicationConfigurationSection)ConfigurationManager.GetSection("applicationSettings/Alkami.Utilities.Settings") ?? CreateDefault();
                return applicationConfiguration;
            }
        }

        private static ApplicationConfigurationSection CreateDefault()
        {
            var config = new ApplicationConfigurationSection
            {
                Environment = new ApplicationConfigurationSection.EnvironmentElement(),
                Logging = new ApplicationConfigurationSection.LoggingElement
                {
                    Encryption = new ApplicationConfigurationSection.EncryptionElement()
                }
            };

            return config;
        }
    }
}
