using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class PermissionFilterValidator : EntityValidatorImpl<PermissionFilter>
	{
		protected override List<ValidationResult> ValidateInternal(PermissionFilter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}