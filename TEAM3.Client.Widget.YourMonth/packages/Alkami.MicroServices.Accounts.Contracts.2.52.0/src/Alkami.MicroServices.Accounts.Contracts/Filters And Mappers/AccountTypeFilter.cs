using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// <see cref="IFilter"/> implementation for <see cref="AccountType"/>
	/// </summary>
	[DataContract(IsReference = true)]
	public class AccountTypeFilter : IFilter
	{
		/// <summary>
		/// Filter on <see cref="AccountType.Id"/>
		/// </summary>
		[DataMember]
		public List<long> Ids { get; set; }

		/// <summary>
		/// Filter on <see cref="AccountType.CoreName"/>
		/// </summary>
		[DataMember]
		public string CoreName { get; set; }
    }
}