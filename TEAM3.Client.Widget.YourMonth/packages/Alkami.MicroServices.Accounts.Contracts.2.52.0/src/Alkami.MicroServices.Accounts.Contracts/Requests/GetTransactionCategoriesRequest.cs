using System;
using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    [Obsolete]
    public class GetTransactionCategoriesRequest : BaseGetRequest<TransactionCategoryFilter, TransactionCategoryMapper, DefaultOrderer>
    {
    }
}