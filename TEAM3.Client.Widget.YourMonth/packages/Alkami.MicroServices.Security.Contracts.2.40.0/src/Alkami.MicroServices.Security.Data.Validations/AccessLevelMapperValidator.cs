using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class AccessLevelMapperValidator : EntityValidatorImpl<AccessLevelMapper>
	{
		protected override List<ValidationResult> ValidateInternal(AccessLevelMapper src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}