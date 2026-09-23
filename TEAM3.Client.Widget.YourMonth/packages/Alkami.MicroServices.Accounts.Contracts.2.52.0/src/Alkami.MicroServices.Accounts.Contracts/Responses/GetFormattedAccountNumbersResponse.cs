using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    /// <summary>
    /// Response for the Get Formatted Account Numbers request.
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetFormattedAccountNumbersResponse : BaseResponse
    {
        /// <summary>
        /// List of formatted accounts.
        /// </summary>
        [DataMember]
        public List<FormattedAccountInformation> FormattedAccounts { get; set; }
    }
}
