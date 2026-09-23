using Alkami.Data.Validations;
using System.Collections.Generic;
using Alkami.MicroServices.Security.Contracts.Filters_And_Mappers;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class JointOwnerFilterValidator : EntityValidatorImpl<JointOwnerFilter>
	{
		protected override List<ValidationResult> ValidateInternal(JointOwnerFilter src)
		{
			var results = new List<ValidationResult>();

		    if (src.AccountIds == null || src.AccountIds.Count == 0)
		    {
		        results.AddValidationError(null, "AccountIds must contain at least one AccountId on the request filter", SubCode.BadRequest);
            }

			return results;
		}
	}
}