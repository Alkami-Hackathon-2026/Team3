using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.Contracts
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class EmptyFilter : IFilter
    {
        [DataMember]
        public List<long> Ids { get; set; }
    }
}