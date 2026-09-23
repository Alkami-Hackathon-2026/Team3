using System;
using System.Runtime.Serialization;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// The exception thrown to indicate that no instances of a service were found
    /// </summary>
#if NET6_0_OR_GREATER
    public class ServiceInstanceNotFoundException : Exception
#else
    public class ServiceInstanceNotFoundException : System.Management.Instrumentation.InstanceNotFoundException
#endif
    {
        /// <summary>
        /// Initializes a new instance of the ServiceInstanceNotFoundException class
        /// </summary>
        public ServiceInstanceNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceInstanceNotFoundException class with its message
        /// string set to message.
        /// </summary>
        /// <param name="message">A string that contains the error message that explains the reason for the exception.</param>
        public ServiceInstanceNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceInstanceNotFoundException class with the specified
        /// error message and the inner exception.
        /// </summary>
        /// <param name="message">A string that contains the error message that explains the reason for the exception.</param>
        /// <param name="innerException">The Exception that caused the current exception to be thrown.</param>
        public ServiceInstanceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceInstanceNotFoundException class with the specified
        /// serialization information and streaming context.
        /// </summary>
        /// <param name="info">The SerializationInfo that contains all the data required to serialize the exception.</param>
        /// <param name="context">The StreamingContext that specifies the source and destination of the stream.</param>
        protected ServiceInstanceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
