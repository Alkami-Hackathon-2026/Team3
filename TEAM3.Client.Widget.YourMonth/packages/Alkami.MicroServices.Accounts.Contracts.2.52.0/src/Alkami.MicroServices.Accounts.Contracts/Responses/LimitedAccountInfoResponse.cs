using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class LimitedAccountInfoResponse: BaseResponse
    {
        [DataMember(EmitDefaultValue = true)]
        public List<LimitedAccountInfo> LimitedAccountInfos { get; set; }
    }
}