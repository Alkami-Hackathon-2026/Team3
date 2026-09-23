using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.Contracts
{
    public interface IFilter
    {
        [DataMember]
        List<long> Ids { get; set; }
    }

}