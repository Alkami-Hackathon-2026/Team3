using System;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract]
    [Obsolete]
    public enum TransactionDateFields
    {
        /// <summary>
        /// The effective date
        /// </summary>
        [EnumMember(Value = "EffectiveDate")]
        EffectiveDate,

        /// <summary>
        /// The posting date
        /// </summary>
        [EnumMember(Value = "PostingDate")]
        PostingDate,
    }
}