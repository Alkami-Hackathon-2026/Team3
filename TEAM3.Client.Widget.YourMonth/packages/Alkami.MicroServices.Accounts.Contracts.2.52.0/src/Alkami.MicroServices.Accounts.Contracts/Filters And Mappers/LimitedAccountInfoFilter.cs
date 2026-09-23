using Alkami.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Filters_And_Mappers
{
    [DataContract(IsReference = true)]
    public class LimitedAccountInfoFilter : IFilter
    {
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
        /// A filter on <see cref="Account.AccountIdentifier"/>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<Guid> AccountIdentifiers { get; set; }
    }
}
