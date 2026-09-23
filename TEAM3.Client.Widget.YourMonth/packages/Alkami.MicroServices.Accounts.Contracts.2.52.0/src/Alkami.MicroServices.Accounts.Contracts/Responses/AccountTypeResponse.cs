using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class AccountTypeResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = true)]
        public List<AccountType> AccountTypes { get; set; }
    }
}