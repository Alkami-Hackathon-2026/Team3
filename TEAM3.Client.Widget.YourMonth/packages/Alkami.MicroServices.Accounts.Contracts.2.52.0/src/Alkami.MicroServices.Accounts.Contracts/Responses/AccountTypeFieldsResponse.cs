using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Requests;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    /// <summary>
    /// Returned response for a <see cref="AddOrUpdateAccountTypeFieldsRequest"/> 
    /// </summary>
    [DataContract(IsReference = true)]
    public class AccountTypeFieldsResponse : BaseResponse
    {
        /// <summary>
        /// List of configured fields that indicates what details, or properties of an Account need to be displayed.
        /// Example Fields: AvailableBalance, InterestRate, InterestPaidYTD, NextPaymentDate, etc.
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<AccountTypeField> Fields { get; set; }
    }
}
