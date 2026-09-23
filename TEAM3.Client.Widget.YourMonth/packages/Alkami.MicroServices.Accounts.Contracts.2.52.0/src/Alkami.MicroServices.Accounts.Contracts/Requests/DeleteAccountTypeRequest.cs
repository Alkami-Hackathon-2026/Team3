using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class DeleteAccountTypeRequest : BaseRequest
    {
        [DataMember]
        public List<long> Ids { get; set; }

        public DeleteAccountTypeRequest()
        {
            Ids = new List<long>();
        }
    }
}
