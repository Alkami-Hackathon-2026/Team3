using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class UserFilterValidator : EntityValidatorImpl<UserFilter>
	{
		protected override List<ValidationResult> ValidateInternal(UserFilter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}