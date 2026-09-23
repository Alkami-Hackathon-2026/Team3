using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Security.Data.Validations
{
    [ExcludeFromCodeCoverage]
    public class GetFlavorUserMetadataRequestValidator : EntityValidatorImpl<GetFlavorUserMetadataRequest>
    {
        protected override List<ValidationResult> ValidateInternal(GetFlavorUserMetadataRequest src)
        {
            // no special validation needed, but we can suppress the validator error by having this
            return new List<ValidationResult>();
        }
    }
}
