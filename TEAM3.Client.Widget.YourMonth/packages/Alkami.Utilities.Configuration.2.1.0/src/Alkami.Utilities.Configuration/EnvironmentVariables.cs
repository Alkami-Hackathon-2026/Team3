using System.Linq;

namespace Alkami.Utilities.Configuration
{
    internal class EnvironmentVariables : SettingsBase
    {
        #region Implementation of ISettings
        /// <inheritdoc />
        protected internal override string Get(string name)
        {
            return System.Environment.GetEnvironmentVariable(name);
        }

        /// <inheritdoc />
        protected internal override string[] AllKeys()
        {
            return System.Environment.GetEnvironmentVariables().Keys.Cast<string>().ToArray();
        }
        #endregion
    }
}