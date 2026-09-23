using Alkami.Contracts;
using System.Runtime.Serialization;
using Alkami.MicroServices.Accounts.Data;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    /// <summary>
    /// Mapping class to determine depth of AccountTypeClass tree
    /// </summary>
    [DataContract(IsReference = true)]
    public class AccountTypeClassMapper : IMapping
    {
        /// <summary>
        /// Informs the service to include <see cref="AccountType"/> with each <see cref="AccountTypeClass"/>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeAccountTypes { get; set; }
    }
}