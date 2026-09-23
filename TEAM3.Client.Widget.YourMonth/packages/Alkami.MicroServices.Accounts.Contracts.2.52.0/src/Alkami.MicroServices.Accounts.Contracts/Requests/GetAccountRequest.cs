using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Sorters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// account identifying info will be obtained for the filters
	/// </summary>
	[DataContract(IsReference = true)]
	public class GetAccountRequest : BaseGetRequest<AccountFilter, AccountMapper, AccountSorter>
	{

		public GetAccountRequest()
		{
			Sorter = new AccountSorter()
			{
				OrderByFields = new List<AccountSortingFields>()
				{
					AccountSortingFields.Id
				}
			};
            Filter = new AccountFilter();
            Mapping = new AccountMapper();
		}
	}
}