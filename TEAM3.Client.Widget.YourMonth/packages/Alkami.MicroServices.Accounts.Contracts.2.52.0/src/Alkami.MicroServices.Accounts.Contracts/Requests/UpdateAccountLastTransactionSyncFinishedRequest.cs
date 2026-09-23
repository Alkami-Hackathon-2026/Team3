using Alkami.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class UpdateAccountLastTransactionSyncFinishedRequest : BaseRequest
    {
        /// <summary>
        /// Account Identifier to be updated
        /// </summary>
        [DataMember]
        public List<Guid> AccountIdentifiers { get; set; }

        /// <summary>
        /// Value to set the LastTransactionSyncFinished column in core.Account (Validated using UTC).
        /// </summary>
        [DataMember]
        public DateTime LastTransactionSyncFinishedDate { get; set; }

    }
}
