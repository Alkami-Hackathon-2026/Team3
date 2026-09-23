using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class EntityFilterValidator : EntityValidatorImpl<EntityFilter>
	{
		protected override List<ValidationResult> ValidateInternal(EntityFilter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}