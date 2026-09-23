using Common.Logging;
using System;
using System.Collections.Generic;

namespace Alkami.Utilities.Extensions
{
    using static TypeConversion.TypeConversionUtility;

    /// <summary>
    /// A tiny utility for reading configuration settings from various sources (this is source-agnostic and technology-agnostic)
    /// </summary>
    public static class SettingReaderUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SettingReaderUtility));

        private static readonly string Msg1 = "Setting for {0} (of type {1}) was {2}";
        private static readonly string Msg2 = "{0}found{1}. Returning {2} Value={3}";

        private static readonly Func<string, string, string, string> msgFct = (name, type, rest) => string.Format(Msg1, name, type, rest);
        private static readonly Func<object, bool, bool, bool, string> restOfMsgFct = (val, found, empty, isDefault) =>
            string.Format(Msg2,
            found ? "" : "NOT ",
            empty ? " but is null/empty" : "",
            isDefault ? "Default" : "Configured",
            val);


        /// <summary>
        /// Returns the configured setting for a given setting key or the default value returned by the provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the setting item</typeparam>
        /// <param name="settings">The keyed collectino of settings to search through</param>
        /// <param name="name">The name/key of the setting</param>
        /// <param name="defaultValueDelegate">The delegate function that would return the default value in case a configured one is not found.</param>
        /// <returns>The configured or default value of the setting identified by its name (key)</returns>
        public static T GetSettingOrDefault<T>(this IDictionary<string, string> settings,
            string name,
            Func<T> defaultValueDelegate)
        {
            return settings.GetSettingOrDefault(name, defaultValueDelegate, false);
        }


        /// <summary>
        /// Returns the configured setting for a given setting key or the default value returned by the provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the setting item</typeparam>
        /// <param name="settings">The keyed collectino of settings to search through</param>
        /// <param name="name">The name/key of the setting</param>
        /// <param name="defaultValueDelegate">The delegate function that would return the default value in case a configured one is not found.</param>
        /// <param name="allowEmptyOrWhitespace">When true, will only use defaultValue if setting is missing or is null. When false, will also return defaultValue if setting is Empty or Whitespace</param>
        /// <returns>The configured or default value of the setting identified by its name (key)</returns>
        public static T GetSettingOrDefault<T>(this IDictionary<string, string> settings, 
            string name, 
            Func<T> defaultValueDelegate,
            bool allowEmptyOrWhitespace)
        {
            Func<object, bool, bool, bool, string> msg = (val, found, empty, isDefault) =>
                msgFct(name, typeof(T).Name, restOfMsgFct(val, found, empty, isDefault));

            T defaultValue = defaultValueDelegate();

            if (settings == null || !(settings).ContainsKey(name))
            {
                Logger.Trace(msg(defaultValue, false, false, true));
                return defaultValue;
            }

            var setting = settings[name];

            try
            {
                if ((allowEmptyOrWhitespace && setting == null) ||
                    (!allowEmptyOrWhitespace && string.IsNullOrWhiteSpace(Convert.ToString(setting))))
                {
                    Logger.Trace(msg(defaultValue, true, true, true));
                    return defaultValue;
                }

                var result = ConvertTo<T>(setting);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Conversion of Setting Value {setting} to Type {typeof(T).Name} has failed. {msg(defaultValue, true, false, true)}. Exception is {ex}");
                return defaultValue;
            }
        }


    }
}
