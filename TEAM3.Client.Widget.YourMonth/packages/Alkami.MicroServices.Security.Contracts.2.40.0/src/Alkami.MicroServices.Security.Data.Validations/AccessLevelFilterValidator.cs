using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class AccessLevelFilterValidator : EntityValidatorImpl<AccessLevelFilter>
	{
		protected override List<ValidationResult> ValidateInternal(AccessLevelFilter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}