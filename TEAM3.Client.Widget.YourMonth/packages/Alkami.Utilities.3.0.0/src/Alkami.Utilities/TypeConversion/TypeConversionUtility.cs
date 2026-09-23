using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Alkami.Utilities.TypeConversion
{
    /// <summary>
    /// Utility that performs (simple) type conversions used mainly for settings retrieval and strong-typing
    /// </summary>
    public static class TypeConversionUtility
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> Converters =
            new ConcurrentDictionary<Type, TypeConverter>();

        /// <summary>
        /// New conversion function implementation. Works with value types (including enums), nullable types, strings, other simple types that can be passed in for settings.
        /// It is made public so that it can be used outside of the settings context as needed.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="o">The object to attempt to convert</param>
        /// <returns>An instance of type T representing the result of conversion of the input object.</returns>
        public static T ConvertTo<T>(object o)
        {
            var t = typeof(T);
            if (o == null)
                return (T)(object)null;

            if (t.IsInstanceOfType(o))
            {
                return (T)o;
            }

            var converter = Converters.GetOrAdd(t, TypeDescriptor.GetConverter);
            return (T)converter.ConvertFromInvariantString(o.ToString());
        }
    }
}
