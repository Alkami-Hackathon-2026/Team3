using System;
using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    [Obsolete]
    public class AddOrUpdateTransactionRequest : BaseCreateOrUpdateRequest<Transaction>
    {
    }
}