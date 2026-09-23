using System;
using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Responses
{
    [DataContract(IsReference = true)]
    [Obsolete]
    public class TransactionCategoryResponse : BaseResponse
    {
        [DataMember(EmitDefaultValue = true)]
        public List<TransactionCategory> TransactionCategories { get; set; }
    }
}