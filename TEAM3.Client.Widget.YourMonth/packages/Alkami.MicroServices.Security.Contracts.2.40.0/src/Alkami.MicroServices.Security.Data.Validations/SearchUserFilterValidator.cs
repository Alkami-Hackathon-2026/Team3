using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts;

namespace Alkami.MicroServices.Security.Data.Validations
{
    /// <summary>
    /// SearchUserFilterValidator
    /// </summary>
    public class SearchUserFilterValidator : EntityValidatorImpl<SearchUserFilter>
    {
        private const string ErrorTemplate = "{0} is not null but must have a length of 4 or greater.";
        private static readonly List<PropertyInfo> FilterProps = 
            typeof(SearchUserFilter).GetProperties().Where(p => p.PropertyType == typeof(SearchUserFilter.SearchField)).ToList();

        /// <inheritdoc cref="EntityValidatorImpl"/>
        protected override List<ValidationResult> ValidateInternal(SearchUserFilter src)
        {
            var results = new List<ValidationResult>();

            if (src is null || src.IsEmpty)
            {
                results.Add(new ValidationResult { Severity = Severity.Error, Message = "SearchUserFilter cannot be null or empty", SubCode = SubCode.BadRequest });
                return results;
            }

            foreach (var prop in FilterProps)
            {
                if (prop.GetValue(src, null) is SearchUserFilter.SearchField propVal && !propVal.IsValid)
                    results.Add(new ValidationResult { Severity = Severity.Error, Message = string.Format(ErrorTemplate, prop.Name), SubCode = SubCode.BadRequest });
            }

            return results;
        }
    }
}
