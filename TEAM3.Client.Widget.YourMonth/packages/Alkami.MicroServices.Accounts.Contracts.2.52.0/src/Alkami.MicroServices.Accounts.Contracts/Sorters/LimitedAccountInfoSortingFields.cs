using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
    [DataContract]
	public enum LimitedAccountInfoSortingFields
	{
		[EnumMember]
		Id = 0,

		[EnumMember]
		LastTransactionDate = 1,

		[EnumMember]
		CreateDate = 2
	}
}