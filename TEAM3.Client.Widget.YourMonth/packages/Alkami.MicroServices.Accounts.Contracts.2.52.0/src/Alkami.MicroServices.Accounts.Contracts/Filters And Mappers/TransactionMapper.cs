using System;
using System.Runtime.Serialization;
using Alkami.Contracts;

namespace Alkami.MicroServices.Accounts.Contracts.Requests
{
	[DataContract(IsReference = true)]
    [Obsolete]
    public class TransactionMapper : IMapping
    {
    }
}