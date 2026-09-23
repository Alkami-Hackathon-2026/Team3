using System.Collections.Generic;
using System.Linq;
using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.Security;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class AddOrUpdateEntityRequestValidator : EntityValidatorImpl<AddOrUpdateEntityRequest>
    {
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateEntityRequest request)
        {
            var results = new List<ValidationResult>();

            if (request.IsServiceAdmin())
                return results;

            if (!request.IsAuthenticated())
            {
                results.Add(GetNoPermissionValidationResult());
            }
            else
            {
                if (!request.IsAdmin() && !request.IsMasterUser() && !request.IsBusinessSubUserWithPermission())
                {
                    results.Add(GetNoPermissionValidationResult());
                }
                else if (request.IsMasterUser() || request.IsBusinessSubUserWithPermission())
                {
                    var entityId = request.GetEntityId();
                    if (request.ItemList.Any(x => x.Id != entityId))
                    {
                        results.Add(GetNoPermissionValidationResult());
                    }
                }
            }

            return results;
        }

        private ValidationResult GetNoPermissionValidationResult()
        {
            return new ValidationResult
            {
                Severity = Severity.Error,
                Message = "User doesn't have permission to modify entity members."
            };
        }

        private ValidationResult GetMultipleEntityValidationError()
        {
            return new ValidationResult
            {
                Severity = Severity.Error,
                Message = "You cannot modify entities members that belong to a different entity than your own."
            };
        }
    }
}
