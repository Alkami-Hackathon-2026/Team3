using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class EntityMapperValidator : EntityValidatorImpl<EntityMapper>
	{
		protected override List<ValidationResult> ValidateInternal(EntityMapper src)
		{
			var results = new List<ValidationResult>();

			if (src.IncludeEntityGroupAccountPermissions.GetValueOrDefault() && !src.IncludeEntityGroupAccounts.GetValueOrDefault())
			{
				src.IncludeEntityGroupAccounts = true;
				results.AddValidationWarning(nameof(src.IncludeEntityGroupAccounts), $"Setting {nameof(src.IncludeEntityGroupAccounts)} to 'true' because child objects were requested.");
			}

			if ((src.IncludeEntityGroupAccounts.GetValueOrDefault()
				|| src.IncludeEntityGroupPermissions.GetValueOrDefault()
				|| src.IncludeEntityGroupMembers.GetValueOrDefault()) && !src.IncludeEntityGroups.GetValueOrDefault())
			{
				src.IncludeEntityGroups = true;
				results.AddValidationWarning(nameof(src.IncludeEntityGroupAccounts), $"Setting {nameof(src.IncludeEntityGroups)} to 'true' because child objects were requested.");
			}

			return results;
		}
	}
}