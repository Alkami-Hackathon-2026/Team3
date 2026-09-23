using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Accounts.Contracts.Sorters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class GetLimitedAccountInfoRequest : BaseGetRequest<LimitedAccountInfoFilter, LimitedAccountInfoMapper, LimitedAccountInfoSorter>
    {
        public GetLimitedAccountInfoRequest()
		{
			Sorter = new LimitedAccountInfoSorter()
			{
				OrderByFields = new List<LimitedAccountInfoSortingFields>()
				{
					LimitedAccountInfoSortingFields.Id
				}
			};
			Filter = new LimitedAccountInfoFilter();
			Mapping = new LimitedAccountInfoMapper();
		}
	}
}
