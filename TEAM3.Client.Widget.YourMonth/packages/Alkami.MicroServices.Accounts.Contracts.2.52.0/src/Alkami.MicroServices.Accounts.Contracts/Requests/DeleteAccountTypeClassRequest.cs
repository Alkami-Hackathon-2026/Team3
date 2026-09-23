using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class DeleteAccountTypeClassRequest : BaseRequest
    {
        [DataMember]
        public List<long> Ids { get; set; }

        public DeleteAccountTypeClassRequest()
        {
            Ids = new List<long>();
        }
    }
}
