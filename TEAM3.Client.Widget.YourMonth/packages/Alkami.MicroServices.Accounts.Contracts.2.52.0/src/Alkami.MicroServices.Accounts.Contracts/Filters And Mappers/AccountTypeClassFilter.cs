using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// <see cref="IFilter"/> implementation for <see cref="AccountTypeClass"/>
	/// </summary>
	[DataContract(IsReference = true)]
	public class AccountTypeClassFilter : IFilter
	{
		/// <summary>
		/// Filter for <see cref="AccountTypeClass.Id"/>
		/// </summary>
		[DataMember]
		public List<long> Ids { get; set; }
	}
}