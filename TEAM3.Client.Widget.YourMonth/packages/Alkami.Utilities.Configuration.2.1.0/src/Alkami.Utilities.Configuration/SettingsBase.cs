namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// Base class that is need to register a Settings setup with <see cref="Manager"/>
    /// </summary>
    public abstract class SettingsBase
    {
        /// <summary>
        /// Get a setting by it's <paramref name="name"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal abstract string Get(string name);
        /// <summary>
        /// Get list of setting names
        /// </summary>
        /// <returns></returns>
        protected internal abstract string[] AllKeys();
    }
}