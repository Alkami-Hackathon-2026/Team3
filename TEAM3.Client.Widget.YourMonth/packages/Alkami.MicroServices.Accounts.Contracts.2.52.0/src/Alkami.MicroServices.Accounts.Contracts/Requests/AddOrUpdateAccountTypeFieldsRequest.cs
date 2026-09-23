using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    /// <summary>
    /// Request for creating or updating Account Type Fields
    /// </summary>
    [DataContract(IsReference = true)]
    public class AddOrUpdateAccountTypeFieldsRequest : BaseCreateOrUpdateRequest<AccountTypeField>
    {
    }
}
