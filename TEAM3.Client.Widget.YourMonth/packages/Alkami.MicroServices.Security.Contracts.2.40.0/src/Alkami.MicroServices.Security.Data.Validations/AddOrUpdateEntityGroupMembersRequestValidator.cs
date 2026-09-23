using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.Security;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class AddOrUpdateEntityGroupMemberRequestValidator : EntityValidatorImpl<AddOrUpdateEntityGroupMemberRequest>
    {
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateEntityGroupMemberRequest request)
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
                if (request.IsAdmin())
                {
                    if (!request.HasPermission(Alkami.Security.Permission.ManageStaff))
                    {
                        results.Add(GetNoPermissionValidationResult());
                    }
                }
                else if (!request.IsMasterUser() && !request.IsBusinessSubUserWithPermission())
                {
                    results.Add(GetNoPermissionValidationResult());
                }
            }

            return results;
        }

      
        private ValidationResult GetNoPermissionValidationResult()
        {
            return new ValidationResult()
            {
                Severity = Severity.Error,
                Message = "User doesn't have permission to modify entity group members."
            };
        }

        private ValidationResult GetMultipleEntityValidationError()
        {
            return new ValidationResult()
            {
                Severity = Severity.Error,
                Message = "You cannot modify entities group members that belong to a different entity than your own."
            };
        }
    }
}
