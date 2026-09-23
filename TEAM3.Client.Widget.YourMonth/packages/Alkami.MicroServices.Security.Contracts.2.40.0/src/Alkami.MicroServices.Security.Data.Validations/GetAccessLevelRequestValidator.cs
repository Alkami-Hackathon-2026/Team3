using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class GetAccessLevelRequestValidator : EntityValidatorImpl<GetAccessLevelRequest>
	{
		protected override List<ValidationResult> ValidateInternal(GetAccessLevelRequest src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}