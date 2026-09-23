using System;

namespace Alkami.Utilities.Exceptions
{
    /// <summary>
    /// Exception that indications that indicates missing configuration
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Intializes a new instance of the <see cref="ConfigurationException"/> class
        /// </summary>
        public ConfigurationException(string message) : base(message)
        {
        }


    }
}
