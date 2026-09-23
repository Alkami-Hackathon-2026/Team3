using Alkami.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    [Obsolete]
    public class TransactionFilter : IFilter
    {
        [DataMember(EmitDefaultValue = true)]
        public List<long> AccountIds { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? AccountTypeClassId { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public TransactionDateFields? DateSearchField { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public bool? Debit { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? EndCheckNumber { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public DateTime? EndDate { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public List<long> Ids { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public decimal? MaximumAmount { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public decimal? MinimumAmount { get; set; }

        //public PagingOrdering<TransactionOrderingFields?> pagingParameters { get; set; }
        [DataMember(EmitDefaultValue = true)]
        public long? PayeeId { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? StartCheckNumber { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public DateTime? StartDate { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? StartingAccountTransactionId { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? TransactionCategoryId { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public long? TransactionTypeId { get; set; }
    }
}