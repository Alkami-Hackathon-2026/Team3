using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class DeleteAccountTypeResponse : BaseResponse
    {
    }
}
