using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
	[DataContract]
	public enum RoutingNumberInfoSortingField
	{
		[EnumMember]
		Id = 0,

		[EnumMember]
		BankName = 1,

		[EnumMember]
		RoutingNumber = 2
	}
}