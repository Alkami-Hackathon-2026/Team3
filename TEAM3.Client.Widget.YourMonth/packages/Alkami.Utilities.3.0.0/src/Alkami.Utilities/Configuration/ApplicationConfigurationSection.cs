using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The ApplicationConfigurationSection class is used to configure application configuration values
    /// for Alkami applications.
    /// </summary>
    /// <remarks>
    /// Here is an example of the configuration section within the application configuration file.
    /// <code>
    /// &lt;?xml version="1.0" encoding="utf-8" ?&gt;
    ///&lt;configuration&gt;
    ///  &lt;configSections&gt;
    ///    &lt;sectionGroup name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"&gt;
    ///      &lt;section name="Alkami.Utilities.Settings" type="Alkami.Utilities.Configuration.ApplicationConfigurationSection, Alkami.Utilities"/&gt;
    ///    &lt;/sectionGroup&gt;
    ///  &lt;/configSections&gt;
    ///  &lt;applicationSettings&gt;
    ///    &lt;Alkami.Utilities.Settings name="My Application"&gt;
    ///      &lt;environment
    ///        name="Unknown"
    ///        type="Unknown"
    ///        hosting="Unknown"
    ///        server="Unknown"
    ///        application="Unknown"
    ///      /&gt;
    ///      &lt;logging
    ///        sanitize="true"
    ///        taxIdRegex="\b([0-9]{1,3})([ -]?)([0-9]{2,3})([ -]?)([0-9]{4})([a-zA-Z]?)\b"
    ///        privateNumberRegex="\b(?&lt;![0-9\*\-][:\s]?)([0-9]{2,19})(?![:\s]?[0-9\*\-])\b"
    ///        logCallStack="false"
    ///      &gt;
    ///        &lt;encryption encrypt="true"
    ///                    store="My"
    ///                    location="CurrentUser"
    ///                    findType="FindBySubjectName"
    ///                    findValue="Alkami Mutual Service"
    ///        /&gt;
    ///      &lt;/logging&gt;
    ///    &lt;/Alkami.Utilities.Settings&gt;
    ///  &lt;/applicationSettings&gt;
    ///&lt;/configuration&gt;
    /// </code>
    /// </remarks>
    public class ApplicationConfigurationSection : ConfigurationSection
    {
        private const string ApplicationNameAppSettingName = "Application.Name";

        /// <summary>
        /// Gets the application settings using the AppSettingsFunc delegate.
        /// </summary>
        /// <value>The application settings.</value>
        private NameValueCollection AppSettings
        {
            get { return ApplicationConfiguration.AppSettingsFunc(); }
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = false)]
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(AppSettings[ApplicationNameAppSettingName]))
                    return AppSettings[ApplicationNameAppSettingName];
                return (string)this["name"];
            }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the application environment element that configures environmental settings.
        /// </summary>
        /// <value>The application environment element.</value>
        [ConfigurationProperty("environment")]
        public EnvironmentElement Environment
        {
            get { return (EnvironmentElement)this["environment"]; }
            set { this["environment"] = value; }
        }

        /// <summary>
        /// Gets or sets the application logging configuration element.
        /// </summary>
        /// <value>The application logging configuration.</value>
        [ConfigurationProperty("logging")]
        public LoggingElement Logging
        {
            get { return (LoggingElement)this["logging"]; }
            set { this["logging"] = value; }
        }

        /// <summary>
        /// The EnvironmentElement class is used to configure environmental application
        /// properties such as the name of the environment, environment type, hosting type and
        /// server type.
        /// </summary>
        /// <seealso cref="System.Configuration.ConfigurationElement" />
        public class EnvironmentElement : ConfigurationElement
        {
            private const string EnvironmentNameAppSettingName = "Environment.Name";
            private const string EnvironmentTypeAppSettingName = "Environment.Type";
            private const string EnvironmentHostingAppSettingName = "Environment.Hosting";
            private const string EnvironmentServerAppSettingName = "Environment.Server";
            private const string EnvironmentApplicationAppSettingName = "Environment.Application";

            /// <summary>
            /// Gets the application settings using the AppSettingsFunc delegate.
            /// </summary>
            /// <value>The application settings.</value>
            private NameValueCollection AppSettings
            {
                get { return ApplicationConfiguration.AppSettingsFunc(); }
            }

            /// <summary>
            /// Gets or sets the name of the environment such POD1, LANEA, AWS_POD1
            /// </summary>
            /// <value>The name of the environment.</value>
            [ConfigurationProperty("name", DefaultValue = "Unknown", IsRequired = false)]
            public String Name
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EnvironmentNameAppSettingName]))
                        return AppSettings[EnvironmentNameAppSettingName];
                    return (String)this["name"];
                }
                set { this["name"] = value; }
            }

            /// <summary>
            /// Gets or sets the environment type such as Development, QA, Staging or Production.
            /// </summary>
            /// <value>The type of the environment.</value>
            [ConfigurationProperty("type", DefaultValue = "Unknown", IsRequired = false)]
            public EnvironmentType Type
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EnvironmentTypeAppSettingName]))
                        return (EnvironmentType)Enum.Parse(typeof(EnvironmentType), AppSettings[EnvironmentTypeAppSettingName], true);
                    return (EnvironmentType)this["type"];
                }
                set { this["type"] = value; }
            }

            /// <summary>
            /// Gets or sets the type of the hosting environment such as Firehost, AWS, OnPremise or None.
            /// </summary>
            /// <value>The hosting environment.</value>
            [ConfigurationProperty("hosting", DefaultValue = "Unknown", IsRequired = false)]
            public HostingType Hosting
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EnvironmentHostingAppSettingName]))
                        return (HostingType)Enum.Parse(typeof(HostingType), AppSettings[EnvironmentHostingAppSettingName], true);
                    return (HostingType)this["hosting"];
                }
                set { this["hosting"] = value; }
            }

            /// <summary>
            /// Gets or sets the server type such as Web, App, Radium, All or other.
            /// </summary>
            /// <value>The type of server role.</value>
            [ConfigurationProperty("server", DefaultValue = "Unknown", IsRequired = false)]
            public ServerType Server
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EnvironmentServerAppSettingName]))
                        return (ServerType)Enum.Parse(typeof(ServerType), AppSettings[EnvironmentServerAppSettingName], true);
                    return (ServerType)this["server"];
                }
                set { this["server"] = value; }
            }

            /// <summary>
            /// Gets or sets the type of application such as WebClient, AdminClient, Microservice or other.
            /// </summary>
            /// <value>The type of application.</value>
            [ConfigurationProperty("application", DefaultValue = "Unknown", IsRequired = false)]
            public ApplicationType Application
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EnvironmentApplicationAppSettingName]))
                        return (ApplicationType)Enum.Parse(typeof(ApplicationType), AppSettings[EnvironmentApplicationAppSettingName], true);
                    return (ApplicationType)this["application"];
                }
                set { this["application"] = value; }
            }
        }

        /// <summary>
        /// The LoggingElement class is used to configure application wide logging properties
        /// such as whether to sanitize log entries, encrypt log entries, log the call stack
        /// and regular expressions to use for sanitizing log entries.
        /// </summary>
        /// <seealso cref="System.Configuration.ConfigurationElement" />
        public class LoggingElement : ConfigurationElement
        {
            private const string SanitizeAppSettingName = "Logging.Sanitize";
            private const string TaxIdRegexAppSettingName = "Logging.TaxIdRegex";
            private const string PrivateNumberRegexAppSettingName = "Logging.PrivateNumberRegex";
            private const string LogCallStackAppSettingName = "Logging.LogCallStack";

            /// <summary>
            /// Gets the application settings using the AppSettingsFunc delegate.
            /// </summary>
            /// <value>The application settings.</value>
            private NameValueCollection AppSettings
            {
                get { return ApplicationConfiguration.AppSettingsFunc(); }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to sanitize log entries.
            /// </summary>
            /// <value><c>true</c> if log entries should be sanitized; otherwise, <c>false</c>.</value>
            [ConfigurationProperty("sanitize", DefaultValue = "true", IsRequired = false)]
            public bool SanitizeLogEntry
            {
                get {
                    if (!string.IsNullOrEmpty(AppSettings[SanitizeAppSettingName]))
                        return Convert.ToBoolean(AppSettings[SanitizeAppSettingName]);
                    return (bool)this["sanitize"]; }
                set { this["sanitize"] = value; }
            }

            /// <summary>
            /// Gets or sets the regular expression to use to identify tax identifiers for sanitizing.
            /// </summary>
            /// <value>The tax identifier regular expression.</value>
            [ConfigurationProperty("taxIdRegex", DefaultValue = @"\b([0-9]{1,3})([ -]?)([0-9]{2,3})([ -]?)([0-9]{4})([a-zA-Z]?)\b", IsRequired = false)]
            public string TaxIdRegex
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[TaxIdRegexAppSettingName]))
                        return AppSettings[TaxIdRegexAppSettingName];
                    return (string)this["taxIdRegex"];
                }
                set { this["taxIdRegex"] = value; }
            }

            /// <summary>
            /// Gets or sets the regular expression to use to private numbers for sanitizing.
            /// </summary>
            /// <value>The private number regular expression.</value>
            [ConfigurationProperty("privateNumberRegex", DefaultValue = @"\b(?<![0-9\*\-][:\s]?)([0-9]{2,19})(?![:\s]?[0-9\*\-])\b", IsRequired = false)]
            public string PrivateNumberRegex
            {
                get {
                    if (!string.IsNullOrEmpty(AppSettings[PrivateNumberRegexAppSettingName]))
                        return AppSettings[PrivateNumberRegexAppSettingName];
                    return (string)this["privateNumberRegex"]; }
                set { this["privateNumberRegex"] = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the call stack should be logged when the LogCallStack
            /// methods are called. Call stacks can take a large amount of storage and should only be logged
            /// in non production environments or when trying to diagnose difficult problems.
            /// </summary>
            /// <value><c>true</c> if the call stack should be logged; otherwise, <c>false</c>.</value>
            [ConfigurationProperty("logCallStack", DefaultValue = "false", IsRequired = false)]
            public bool LogCallStack
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[LogCallStackAppSettingName]))
                        return Convert.ToBoolean(AppSettings[LogCallStackAppSettingName]);
                    return (bool)this["logCallStack"];
                }
                set { this["logCallStack"] = value; }
            }

            /// <summary>
            /// Gets or sets the encryption element that is used to configure application
            /// encryption properties.
            /// </summary>
            /// <value>The application encryption configuration element.</value>
            [ConfigurationProperty("encryption")]
            public EncryptionElement Encryption
            {
                get { return (EncryptionElement)this["encryption"]; }
                set { this["encryption"] = value; }
            }
        }

        /// <summary>
        /// The EncryptionElement class contains encryption configuration such as
        /// whether encryption is enabled, the Certificate Store Name, Location and
        /// certificate to use for encrypting logging statements.
        /// </summary>
        /// <seealso cref="System.Configuration.ConfigurationElement" />
        public class EncryptionElement : ConfigurationElement
        {
            private const string EncryptAppSettingName = "Logging.Encryption.Encrypt";
            private const string StoreNameAppSettingName = "Logging.Encryption.StoreName";
            private const string StoreLocationAppSettingName = "Logging.Encryption.StoreLocation";
            private const string FindTypeAppSettingName = "Logging.Encryption.FindType";
            private const string FindValueAppSettingName = "Logging.Encryption.FindValue";

            /// <summary>
            /// Gets the application settings using the AppSettingsFunc delegate.
            /// </summary>
            /// <value>The application settings.</value>
            private NameValueCollection AppSettings
            {
                get { return ApplicationConfiguration.AppSettingsFunc(); }
            }

            /// <summary>
            /// Gets or sets a value indicating whether encryption is enabled.
            /// </summary>
            /// <value><c>true</c> if encryption is enabled; otherwise, <c>false</c>.</value>
            [ConfigurationProperty("encrypt", DefaultValue = "false", IsRequired = false)]
            public bool EncryptionEnabled
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[EncryptAppSettingName]))
                        return Convert.ToBoolean(AppSettings[EncryptAppSettingName]);
                    return (bool)this["encrypt"];
                }
                set { this["encrypt"] = value; }
            }

            /// <summary>
            /// Gets or sets the name of the certificate store such as My, TrustedPeople and AuthRoot.
            /// </summary>
            /// <value>The name of the certificate store.</value>
            [ConfigurationProperty("store", DefaultValue = "My", IsRequired = false)]
            public StoreName StoreName
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[StoreNameAppSettingName]))
                        return (StoreName)Enum.Parse(typeof(StoreName), AppSettings[StoreNameAppSettingName], true);
                    return (StoreName)this["store"];
                }
                set { this["store"] = value; }
            }

            /// <summary>
            /// Gets or sets the name of the certificate location such as LocalComputer and CurrentUser.
            /// </summary>
            /// <value>The name of the certificate location.</value>
            [ConfigurationProperty("location", DefaultValue = "LocalMachine", IsRequired = false)]
            public StoreLocation StoreLocation
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[StoreLocationAppSettingName]))
                        return (StoreLocation)Enum.Parse(typeof(StoreLocation), AppSettings[StoreLocationAppSettingName]);
                    return (StoreLocation)this["location"];
                }
                set { this["location"] = value; }
            }

            /// <summary>
            /// Gets or sets the method used to find a certificate such as FindByThumbprint and FindBySubjectName.
            /// The setting interprets the value contained in the <see cref="FindValue"/> property.
            /// </summary>
            /// <value>The method to use to find the certificate to use to encrypt secure log entries.</value>
            [ConfigurationProperty("findType", DefaultValue = "FindBySubjectName", IsRequired = false)]
            public X509FindType FindType
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[FindTypeAppSettingName]))
                        return (X509FindType)Enum.Parse(typeof(X509FindType), AppSettings[FindTypeAppSettingName]);
                    return (X509FindType)this["findType"];
                }
                set { this["findType"] = value; }
            }

            /// <summary>
            /// Gets or sets the value to use to find a certificate. How this value is interpreted
            /// depends on the setting of the <see cref="FindType"/> property.
            /// </summary>
            /// <value>The value to find the certificate from the configured certificate location.</value>
            [ConfigurationProperty("findValue", DefaultValue = "Alkami Mutual Service", IsRequired = false)]
            public string FindValue
            {
                get
                {
                    if (!string.IsNullOrEmpty(AppSettings[FindValueAppSettingName]))
                        return AppSettings[FindValueAppSettingName];
                    return (string)this["findValue"];
                }
                set { this["findValue"] = value; }
            }
        }
    }
}
