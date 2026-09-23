using Alkami.Data.Validations;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.Security;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.MicroServices.Security.Data.Validations
{
	public class GetEntityRequestValidator : EntityValidatorImpl<GetEntityRequest>
	{
		protected override List<ValidationResult> ValidateInternal(GetEntityRequest src)
		{
			var results = new List<ValidationResult>();

			if (!src.IsAdmin())
			{
				if ((src.Filter == null)
					|| (string.IsNullOrWhiteSpace(src.Filter.PartialName)
						&& ((src.Filter.Ids == null) || (src.Filter.Ids.Count < 1))
						&& ((src.Filter.EntityTypeIds == null) || (src.Filter.EntityTypeIds.Count < 1))))
				{
					results.AddValidationError(nameof(src.Filter), "Request is missing appropriate filtering.  Please supply a partial name or a set of Ids.");
				}

				var strEntityId = src.GetClaimValue(AlkamiClaimTypes.EntityId);
				long entityId;

				if (!long.TryParse(strEntityId, out entityId) || src.Filter.Ids.Any(id => id != entityId))
				{
					results.AddValidationError(nameof(src.Filter), $"You do not have access to query for the {nameof(src.Filter.Ids)} supplied.");
				}
			}

			return results;
		}
	}
}