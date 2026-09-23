using System.Collections.Generic;
using System.Linq;
using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class AddOrUpdateUserRequestValidator : EntityValidatorImpl<AddOrUpdateUserRequest>
    {
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateUserRequest src)
        {
            var results = new List<ValidationResult>();

            foreach (var user in src.ItemList)
            {
                if (user.UserAdditionalInfos != null && user.UserAdditionalInfos.GroupBy(x => x.UserAdditionalInfoType).Any(x => x.Count() > 1))
                {
                    results.Add(new ValidationResult()
                    {
                        Severity = Severity.Error,
                        Message = "User can't have more than one of the same UserAdditionalInfoType"
                    });
                }
            }
            
            return results;
        }
    }
}