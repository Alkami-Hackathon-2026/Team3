using System;
using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Accounts.Data;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// <see cref="IMapping"/> implementation for <see cref="Account"/>
	/// </summary>
	[DataContract(IsReference = true)]
	public class AccountMapper : IMapping
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
		/// Indicates if the service should populate <see cref="ExternalAccount.RoutingNumberInfo"/> in the result set
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public bool? IncludeRoutingInfo { get; set; }

		/// <summary>
		/// Indicates if the service should populate <see cref="Transaction.TransactionCode"/>
		/// <remarks>This requires <see cref="IncludeTransactions"/> to be set</remarks>
		/// </summary>
		[Obsolete]
		[DataMember(EmitDefaultValue = true)]
		public bool? IncludeTransactionDetails { get; set; }

		/// <summary>
		/// Indicates if the service should populate <see cref="Account.Transactions"/> in the result set
		/// </summary>
		[Obsolete]
		[DataMember(EmitDefaultValue = true)]
		public bool? IncludeTransactions { get; set; }

		/// <summary>
		/// Indicates if the service should populate the <see cref="MaskedAccountNumber"/> in the result set.
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public AccountMaskSettings AccountMaskSettings { get; set; }
	}
}