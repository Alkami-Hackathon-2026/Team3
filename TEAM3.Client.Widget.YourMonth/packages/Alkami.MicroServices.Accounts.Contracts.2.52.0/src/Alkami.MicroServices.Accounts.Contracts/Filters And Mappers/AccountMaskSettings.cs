using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers
{
	/// <summary>
	/// The <see cref="AccountMaskSettings"/> define how to generate masked account numbers.
	/// </summary>
	[DataContract(IsReference = true)]
	public class AccountMaskSettings
	{
		/// <summary>
		/// Defines how to join the account holder and number.
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public string JoinAccountHolderNumberFormatString { get; set; }

		/// <summary>
		/// Indicates whether formatting should only be applied to the account holder.
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public bool FormatOnlyAccountHolder { get; set; }

		/// <summary>
		/// Indicates whether the output should be padded the full length of the requested format.
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public bool PadOutput { get; set; }
	}
}