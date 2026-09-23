using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class AccountTypeClassResponse : BaseResponse
    {
        public AccountTypeClassResponse()
        {
            AccountTypeClasses = new List<AccountTypeClass>();
        }

        [DataMember(EmitDefaultValue = true)]
        public List<AccountTypeClass> AccountTypeClasses { get; set; }
    }
}