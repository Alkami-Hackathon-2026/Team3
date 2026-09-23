using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
    /// <summary>
    /// Container to determine how to sort Accounts
    /// </summary>
    [DataContract(IsReference = true)]
	public class LimitedAccountInfoSorter : ISortOrder<LimitedAccountInfoSortingFields>
	{
		[DataMember]
		public bool Ascending { get; set; }

		[DataMember]
		public List<LimitedAccountInfoSortingFields> OrderByFields { get; set; }

		public LimitedAccountInfoSorter()
		{
			Ascending = true;
			OrderByFields = new List<LimitedAccountInfoSortingFields>()
			{
				LimitedAccountInfoSortingFields.Id,
			};
		}
	}
}
