using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;
using Alkami.Security;
using System.Linq;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class GetUserRequestValidator : EntityValidatorImpl<GetUserRequest>
	{
		protected override List<ValidationResult> ValidateInternal(GetUserRequest src)
		{
			var results = new List<ValidationResult>();

            if (src != null)
            {
                var masterUserClaimValue = src.GetClaimValue(AlkamiClaimTypes.IsBusinessMasterUser);
                bool isMasterUser = false;
                var entityId = src.GetEntityId();

                if (!string.IsNullOrWhiteSpace(masterUserClaimValue) && bool.TryParse(masterUserClaimValue, out isMasterUser))
                {
                    if (!isMasterUser && !src.IsAdmin() && entityId.GetValueOrDefault() == 0)
                    {
                        if (src.Filter != null && src.Filter.Ids != null && src.Filter.Ids.Any(userId => userId != src.UserId))
                        {
                            results.Add(new ValidationResult()
                            {
                                Severity = Severity.Error,
                                Message = "The Filter contains ids that the requester doesn't have access to."
                            });
                        }
                    }
                }
            }

            return results;
		}
	}
}