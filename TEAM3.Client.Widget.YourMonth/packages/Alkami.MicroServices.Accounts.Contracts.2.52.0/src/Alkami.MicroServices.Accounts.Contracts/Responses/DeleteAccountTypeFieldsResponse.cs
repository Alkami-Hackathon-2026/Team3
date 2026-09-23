using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    /// <summary>
    /// Returned response for a <see cref="DeleteAccountTypeFieldsResponse"/> 
    /// </summary>
    [DataContract(IsReference = true)]
    public class DeleteAccountTypeFieldsResponse : BaseResponse
    {
    }
}
