using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Filters_And_Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Security.Data.Validations
{
    public class FlavorUserMetadataFilterValidator : EntityValidatorImpl<FlavorUserMetadataFilter>
    {
        protected override List<ValidationResult> ValidateInternal(FlavorUserMetadataFilter src)
        {
            var results = new List<ValidationResult>();

            if(src.FlavorIds?.Any(id => id <= 0) ?? false)
            {
                results.AddValidationError(nameof(src.FlavorIds), "FlavorIds must be greater than zero.", SubCode.BadRequest);
            }

            if((src.FlavorIds?.Count ?? 0) > 2100)
            {
                results.AddValidationError(nameof(src.FlavorIds), "You cannot pass more than 2100 flavorIds in one query.", SubCode.BadRequest);
            }

            return results;
        }
    }
}
