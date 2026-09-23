using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class UserMapperValidator : EntityValidatorImpl<UserMapper>
	{
		protected override List<ValidationResult> ValidateInternal(UserMapper src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}