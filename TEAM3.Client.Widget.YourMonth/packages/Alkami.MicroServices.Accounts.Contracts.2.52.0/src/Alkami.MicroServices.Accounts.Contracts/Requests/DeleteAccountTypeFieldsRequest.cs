using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    /// <summary>
    /// Request for deleting Account Type Fields
    /// </summary>
    [DataContract(IsReference = true)]
    public class DeleteAccountTypeFieldsRequest : BaseRequest
    {
        /// <summary>
        /// The list of account type field ids to delete
        /// </summary>
        [DataMember]
        public List<long> Ids { get; set; }

        /// <inheritdoc />
        public DeleteAccountTypeFieldsRequest()
        {
            Ids = new List<long>();
        }
    }
}
