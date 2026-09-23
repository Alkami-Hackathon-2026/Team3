using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class GetPermissionRequestValidator : EntityValidatorImpl<GetPermissionRequest>
	{
		protected override List<ValidationResult> ValidateInternal(GetPermissionRequest src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}