using System.Runtime.Serialization;
using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// <see cref="IMapping"/> for <see cref="AccountType"/>
	/// </summary>
	/// <seealso cref="Alkami.Contracts.IMapping" />
	[DataContract(IsReference = true)]
	public class AccountTypeMapper : IMapping
    {
        /// <summary>
        /// Indicates if the service should populate the <see cref="AccountTypeField" /> of the result set
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeAccountTypeFields { get; set; }
    }
}