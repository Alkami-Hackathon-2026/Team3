using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alkami.Data.Validations;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class EntityGroupMemberValidator : EntityValidatorImpl<EntityGroupMember>
    {
        protected override List<ValidationResult> ValidateInternal(EntityGroupMember src)
        {
            return new List<ValidationResult>();
        }
    }
}
