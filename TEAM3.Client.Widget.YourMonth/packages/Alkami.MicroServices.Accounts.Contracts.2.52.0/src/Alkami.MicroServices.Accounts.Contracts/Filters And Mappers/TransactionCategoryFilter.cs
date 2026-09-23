using System;
using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Alkami.Contracts.IFilter" />
	[DataContract(IsReference = true)]
    [Obsolete]
	public class TransactionCategoryFilter : IFilter
    {
		/// <summary>
		/// Gets or sets the ids.
		/// </summary>
		/// <value>
		/// The ids.
		/// </value>
		[DataMember]
		public List<long> Ids { get; set; }


		/// <summary>
		/// Gets or sets the Owner user
		/// </summary>
		[DataMember]
		public List<long> OwnerUserIds { get; set; }
    }
}