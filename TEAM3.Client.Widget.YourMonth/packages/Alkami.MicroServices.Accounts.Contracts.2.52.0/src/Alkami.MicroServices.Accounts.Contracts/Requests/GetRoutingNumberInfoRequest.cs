using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Accounts.Contracts.Sorters;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	/// <summary>
	/// Get Routing Number Info Request
	/// </summary>
	[DataContract(IsReference=true)]
	public class GetRoutingNumberInfoRequest : BaseGetRequest<RoutingNumberInfoFilter, EmptyMapper, RoutingNumberInfoSorter>
	{

	}
}
