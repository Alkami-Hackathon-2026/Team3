using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.MicroServices.Accounts.Data;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    /// <summary>
    /// Request for getting formatted account numbers.
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetFormattedAccountNumbersRequest : BaseRequest
    {
        /// <summary>
        /// The accounts to format.
        /// </summary>
        [DataMember]
        public List<AccountInformation> Accounts { get; set; }
    }
}
