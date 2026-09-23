using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class EntitySorterValidator : EntityValidatorImpl<EntitySorter>
	{
		protected override List<ValidationResult> ValidateInternal(EntitySorter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}