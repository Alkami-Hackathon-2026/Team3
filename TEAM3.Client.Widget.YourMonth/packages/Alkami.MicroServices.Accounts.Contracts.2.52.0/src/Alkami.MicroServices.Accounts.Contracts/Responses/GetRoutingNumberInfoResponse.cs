using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference=true)]
    public class GetRoutingNumberInfoResponse : BaseResponse<RoutingNumberInfo>
    {
        public GetRoutingNumberInfoResponse()
        {
            ItemList = new List<RoutingNumberInfo>();
        }
    }
}
