using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
	/// <summary>
	/// Container to determine how to sort Accounts
	/// </summary>
	[DataContract(IsReference = true)]
	public class AccountSorter : ISortOrder<AccountSortingFields>
	{
		[DataMember]
		public bool Ascending { get; set; }

		[DataMember]
		public List<AccountSortingFields> OrderByFields { get; set; }

		public AccountSorter()
		{
			Ascending = true;
			OrderByFields = new List<AccountSortingFields>()
			{
				AccountSortingFields.Id,
			};
		}
	}
}