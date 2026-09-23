using System;
using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="Alkami.Contracts.IMapping" />
	[DataContract(IsReference = true)]
    [Obsolete]
	public class TransactionCategoryMapper : IMapping
	{
		/// <summary>
		///
		/// </summary>
		[DataMember]
		public bool? IncludeAllLevels { get; set; }
	}
}