using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Sorters;
using System.Collections.Generic;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class UserSorterValidator : EntityValidatorImpl<UserSorter>
	{
		protected override List<ValidationResult> ValidateInternal(UserSorter src)
		{
			var results = new List<ValidationResult>();

			return results;
		}
	}
}