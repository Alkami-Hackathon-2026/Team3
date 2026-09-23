using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.Contracts
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class EmptyMapper : IMapping { }
}