using Alkami.MicroServices.Accounts.Data;
using System;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Models
{
    [DataContract(IsReference = true)]
    public class LimitedAccountInfo
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public Guid AccountIdentifier { get; set; }

        [DataMember]
        public long AccountTypeId { get; set; }

        [DataMember]
        public MaskedAccountNumber MaskedAccountNumber { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public virtual AccountType AccountType { get; set; }
    }
}
