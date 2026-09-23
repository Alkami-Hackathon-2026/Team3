using System;
using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.Diagnostics;
#endif

namespace Alkami.Monitoring
{
    /// <summary>
    /// Monitor Event is a concrete implementation of an event's payload information.
    /// </summary>
    public class MonitorEvent : Dictionary<string, object>
    {

#if NET6_0_OR_GREATER

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bankIdentifier"> Bank Identifier of an event.</param>
        /// <param name="userIdentifier"> User Id.</param>
        public MonitorEvent(Guid? bankIdentifier, Guid? userIdentifier) : this(null, bankIdentifier, userIdentifier)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="correlationId"> Correlation Id of an event.</param>
        /// <param name="bankIdentifier"> Bank Identifier of an event.</param>
        /// <param name="userIdentifier"> User Id.</param>
        public MonitorEvent(string correlationId, Guid? bankIdentifier, Guid? userIdentifier)
        {
            if (Activity.Current != null)
            {
                this["TraceId"] = Activity.Current.GetTraceId();
                this["SpanId"] = Activity.Current.GetSpanId();
                this["ParentId"] = Activity.Current.GetParentId();
            }

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                this["CorrelationId"] = correlationId;
            }

            if (bankIdentifier.HasValue)
            {
                if (bankIdentifier == Guid.Empty)
                    throw new ArgumentException("Must not be empty", nameof(bankIdentifier));
                this["BankIdentifier"] = bankIdentifier.Value.ToString();
            }

            if (userIdentifier.HasValue)
            {
                if (userIdentifier == Guid.Empty)
                    throw new ArgumentException("Must not be empty", nameof(userIdentifier));
                this["UserIdentifier"] = userIdentifier.Value.ToString();
            }
        }

#else

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="BankIdentifier"> Bank Identifier of an event.</param>
        /// <param name="CorrelationId"> Correlation Id of an event.</param>
        /// <param name="UserIdentifier"> User Id.</param>
        public MonitorEvent(string CorrelationId, Guid? BankIdentifier, Guid? UserIdentifier)
        {

            if (string.IsNullOrWhiteSpace(CorrelationId))
                throw new ArgumentException("CorrelationId must not be null/empty");
            this["CorrelationId"] = CorrelationId;

            if (BankIdentifier != null)
            {
                if (BankIdentifier == Guid.Empty)
                    throw new ArgumentException("BankIdentifier must not be empty");
                this["BankIdentifier"] = BankIdentifier.ToString();
            }

            if (UserIdentifier != null)
            {
                if (UserIdentifier == Guid.Empty)
                    throw new ArgumentException("UserIdentifier must not be empty");
                this["UserIdentifier"] = UserIdentifier.ToString();
            }
        }

#endif

    }
}
