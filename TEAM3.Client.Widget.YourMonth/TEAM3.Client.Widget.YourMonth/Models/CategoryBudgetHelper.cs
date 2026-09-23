using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TEAM3.Client.Widget.YourMonth.Models
{
    /// <summary>
    /// Parses and serializes the per-user category budget map stored in the
    /// "CategoryBudgets" user widget setting. Pure C#, unit tested in isolation.
    /// </summary>
    public static class CategoryBudgetHelper
    {
        public const int MaxEntries = 100;
        public const int MaxCategoryNameLength = 100;
        public const decimal MaxBudgetAmount = 1000000m;

        /// <summary>
        /// Deserializes a budget map, dropping anything malformed rather than throwing:
        /// blank or oversized names, non-positive or absurd amounts, excess entries.
        /// </summary>
        public static Dictionary<string, decimal> Parse(string json)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            Dictionary<string, decimal> raw;
            try
            {
                raw = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
            }
            catch (JsonException)
            {
                return result;
            }

            foreach (var pair in (raw ?? new Dictionary<string, decimal>()).Take(MaxEntries * 2))
            {
                var name = (pair.Key ?? string.Empty).Trim();
                if (name.Length == 0 || name.Length > MaxCategoryNameLength)
                {
                    continue;
                }

                if (pair.Value <= 0 || pair.Value > MaxBudgetAmount)
                {
                    continue;
                }

                if (result.Count >= MaxEntries)
                {
                    break;
                }

                result[name] = Math.Round(pair.Value, 2);
            }

            return result;
        }

        public static string Serialize(Dictionary<string, decimal> budgets)
        {
            return JsonConvert.SerializeObject(budgets ?? new Dictionary<string, decimal>());
        }

        /// <summary>
        /// Deserializes a JSON array of category names (used for the removed-categories list),
        /// dropping blanks and oversized names and de-duplicating case-insensitively.
        /// </summary>
        public static List<string> ParseNameList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            List<string> raw;
            try
            {
                raw = JsonConvert.DeserializeObject<List<string>>(json);
            }
            catch (JsonException)
            {
                return new List<string>();
            }

            return (raw ?? new List<string>())
                .Select(name => (name ?? string.Empty).Trim())
                .Where(name => name.Length > 0 && name.Length <= MaxCategoryNameLength)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxEntries)
                .ToList();
        }

        public static string SerializeNameList(List<string> names)
        {
            return JsonConvert.SerializeObject(names ?? new List<string>());
        }
    }
}
