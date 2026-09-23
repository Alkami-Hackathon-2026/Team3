using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class AccountResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = true)]
        public List<Account> Accounts { get; set; }

    }
}