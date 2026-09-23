using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers
{
    public class LimitedAccountInfoMapper : IMapping
	{
		/// <summary>
		/// Indicates if the service should populate <see cref="Account.AccountType"/> in the result set
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public bool? IncludeAccountType { get; set; }

		/// <summary>
		/// Indicates if the service should populate the <see cref="AccountTypeField" /> of the <see cref="Account.AccountType"/> in the result set
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public bool? IncludeAccountTypeFields { get; set; }

		/// <summary>
		/// Indicates if the service should populate the <see cref="MaskedAccountNumber"/> in the result set.
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public AccountMaskSettings AccountMaskSettings { get; set; }
	}
}
