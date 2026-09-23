using System;
using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.MicroServices.Accounts.Data;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    /// <summary>
    /// <see cref="IFilter"/> implementation to filter <see cref="Account"/>
    /// </summary>
    [DataContract(IsReference = true)]
    public class AccountFilter : IFilter
    {
		/// <summary>
		/// A Dictionary of <see cref="Account.Number"/> and <see cref="Account.AccountHolder"/> If the Holder is null, we will only match on <see cref="Account.Number"/>
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
		public Dictionary<string, int> AccountNumbersAndHolders { get; set; }

        /// <summary>
        /// A list of <see cref="AccountNumberAndHolderPair"/> to filter by.
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<AccountNumberAndHolderPair> AccountNumberAndHolderPairs { get; set; }

        /// <summary>
        /// A list of <see cref="Account.Number"/>'s to filter on
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
		public List<string> AccountNumbers { get; set; }

		/// <summary>
		/// A list of <see cref="Account.Id"/>'s to filter on
		/// </summary>
		[DataMember(EmitDefaultValue = true)]
        public List<long> Ids { get; set; }

        /// <summary>
        /// A filter to indicate whether <see cref="ExternalAccount"/>'s in the result set
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeExternal { get; set; }


        /// <summary>
        /// A filter to indicate whether <see cref="AggregatedAccount"/> is in the result set
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeAggregate { get; set; }

        /// <summary>
        /// A filter to limit the <see cref="ExternalAccount.RoutingNumberInfoId"/>'s in the result set
        /// <remarks>This property requires <see cref="IncludeExternal"/> to be set</remarks>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public long? RoutingNumberInfoId { get; set; }

        /// <summary>
        /// A filter on <see cref="Account.AccountIdentifier"/>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<Guid> AccountIdentifiers { get; set; }

        /// <summary>
        /// A list of <see cref="Account.AccountHolder"/>'s to filter on
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<long> AccountHolders { get; set; }
    }
}