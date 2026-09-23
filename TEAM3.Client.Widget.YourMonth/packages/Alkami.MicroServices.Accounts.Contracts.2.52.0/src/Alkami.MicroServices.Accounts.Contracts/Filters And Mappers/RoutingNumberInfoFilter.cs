using Alkami.Contracts;
using Alkami.MicroServices.Accounts.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers
{
	/// <summary>
	/// Routing Number Info Filter
	/// </summary>
	/// <seealso cref="Alkami.Contracts.IFilter" />
	[DataContract(IsReference=true)]
	public class RoutingNumberInfoFilter : IFilter
	{
		/// <summary>
		/// Gets or sets the ids.
		/// </summary>
		/// <value>
		/// The ids.
		/// </value>
		[DataMember]
		public List<long> Ids { get; set; }

		/// <summary>
		/// Gets or sets the routing numbers.
		/// </summary>
		/// <value>
		/// The routing numbers.
		/// </value>
		[DataMember]
		public List<string> RoutingNumbers { get; set; }

		/// <summary>
		/// Gets or sets the partial name of the bank.
		/// </summary>
		/// <value>
		/// The partial name of the bank.
		/// </value>
		[DataMember]
		public string PartialBankName { get; set; }

		/// <summary>
		/// Gets or sets the is blocked for wires.
		/// </summary>
		/// <value>
		/// The is blocked for wires.
		/// </value>
		[DataMember]
		public bool? IsBlockedForWires { get; set; }

		/// <summary>
		/// Gets or sets the is blocked for ach.
		/// </summary>
		/// <value>
		/// The is blocked for ach.
		/// </value>
		[DataMember]
		public bool? IsBlockedForACH { get; set; }

		/// <summary>
		/// Gets or sets the type of the routing number information.
		/// </summary>
		/// <value>
		/// The type of the routing number information.
		/// </value>
		[DataMember]
		public RoutingNumberInfoType? RoutingNumberInfoType { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingNumberInfoFilter"/> class.
		/// </summary>
		public RoutingNumberInfoFilter()
		{
			Ids = new List<long>();
		}

		public override string ToString()
		{
			return new
			{
				Ids = Ids.Print(),
				PartialBankName,
				IsBlockedForACH,
				IsBlockedForWires,
				RoutingNumberInfoType
			}.ToString();
		}
	}
}
