using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	[DataContract(IsReference = true)]
	public class GetAccountTypeRequest : BaseGetRequest<AccountTypeFilter, AccountTypeMapper, DefaultOrderer>
	{
	}
}