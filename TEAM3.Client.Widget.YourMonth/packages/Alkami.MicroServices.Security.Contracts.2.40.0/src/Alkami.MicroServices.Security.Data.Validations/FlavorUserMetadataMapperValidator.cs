using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Filters_And_Mappers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Security.Data.Validations
{
    [ExcludeFromCodeCoverage]
    public class FlavorUserMetadataMapperValidator : EntityValidatorImpl<FlavorUserMetadataMapper>
    {
        protected override List<ValidationResult> ValidateInternal(FlavorUserMetadataMapper src)
        {
            // suppress errors
            return new List<ValidationResult>();
        }
    }
}
