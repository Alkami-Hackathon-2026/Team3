using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class GetJointOwnerRequestValidator : EntityValidatorImpl<GetJointOwnerRequest>
	{
		protected override List<ValidationResult> ValidateInternal(GetJointOwnerRequest src)
		{
		    var results = new List<ValidationResult>();

		    if (src.Filter == null)
		    {
                results.AddValidationError(null, "The JointOwnerFilter must be provided on the request", SubCode.BadRequest);
		    }

		    return results;
        }
	}
}