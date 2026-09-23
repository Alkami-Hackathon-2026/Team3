using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Sorters
{
	/// <summary>
	/// Fields that are supported for sorting
	/// </summary>
	[DataContract]
	public enum AccountSortingFields
	{
		[EnumMember]
		Id = 0,

		[EnumMember]
		LastTransactionDate = 1,

		[EnumMember]
		CreateDate = 2,
	}
}