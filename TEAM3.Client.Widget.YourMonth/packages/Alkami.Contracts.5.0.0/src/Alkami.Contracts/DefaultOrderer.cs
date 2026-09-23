using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.Contracts
{
    /// <summary>
    /// A simple default orderer
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class DefaultOrderer : ISortOrder
    {
        [DataMember]
        public bool Ascending { get; set; }
    }
}