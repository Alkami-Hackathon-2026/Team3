using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	[DataContract(IsReference = true)]
	public class GetAccountTypeClassRequest : BaseGetRequest<AccountTypeClassFilter, AccountTypeClassMapper, DefaultOrderer>
	{
		public GetAccountTypeClassRequest()
		{
			Filter = new AccountTypeClassFilter();
			Mapping = new AccountTypeClassMapper();
			Sorter = new DefaultOrderer { Ascending = true };
		}
    }
}