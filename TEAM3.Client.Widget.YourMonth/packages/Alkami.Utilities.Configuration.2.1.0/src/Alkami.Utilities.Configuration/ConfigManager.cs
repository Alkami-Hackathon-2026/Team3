using System.Configuration;

namespace Alkami.Utilities.Configuration
{
    internal class ConfigManager : SettingsBase
    {
        /// <inheritdoc />
        protected internal override string Get(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        /// <inheritdoc />
        protected internal override string[] AllKeys()
        {
            return ConfigurationManager.AppSettings.AllKeys;
        }
    }
}