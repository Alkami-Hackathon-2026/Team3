using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alkami.Data.Validations;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class EntityValidator : EntityValidatorImpl<Entity>
    {
        protected override List<ValidationResult> ValidateInternal(Entity src)
        {
            // This is required by the BaseCreateOrUpdate Request
            return new List<ValidationResult>();
        }
    }
}
