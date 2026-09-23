using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class GetAccountTypeFieldsResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = true)]
        public List<string> AccountTypeFields { get; set; }
    }
}
