using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
	[DataContract(IsReference = true)]
	public class RoutingNumberInfoSorter : ISortOrder<RoutingNumberInfoSortingField>
	{
		[DataMember]
		public bool Ascending { get; set; }

		[DataMember]
		public List<RoutingNumberInfoSortingField> OrderByFields { get; set; }

		public RoutingNumberInfoSorter()
		{
			Ascending = true;
			OrderByFields = new List<RoutingNumberInfoSortingField>()
			{
				RoutingNumberInfoSortingField.Id,
			};
		}
	}
}