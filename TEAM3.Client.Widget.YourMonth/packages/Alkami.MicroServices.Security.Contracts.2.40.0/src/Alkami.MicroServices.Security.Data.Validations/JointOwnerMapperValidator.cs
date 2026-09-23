using Alkami.Data.Validations;
using System.Collections.Generic;
using Alkami.MicroServices.Security.Contracts.Filters_And_Mappers;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class JointOwnerMapperValidator : EntityValidatorImpl<JointOwnerMapper>
	{
		protected override List<ValidationResult> ValidateInternal(JointOwnerMapper src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}