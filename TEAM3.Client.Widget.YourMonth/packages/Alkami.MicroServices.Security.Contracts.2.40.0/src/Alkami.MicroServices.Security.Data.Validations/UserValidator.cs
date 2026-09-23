using System;
using System.Collections.Generic;
using Alkami.Data.Validations;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class UserValidator : EntityValidatorImpl<User>
    {
        protected override List<ValidationResult> ValidateInternal(User src)
        {
            var results = new List<ValidationResult>();

            return results;
        }
    }
}
