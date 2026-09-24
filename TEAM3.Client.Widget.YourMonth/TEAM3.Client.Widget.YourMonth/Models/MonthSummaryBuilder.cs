using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TEAM3.Client.Widget.YourMonth.Models
{
    /// <summary>
    /// Turns a raw transaction list into the "Your Month" summary.
    /// Pure C#: no Alkami dependencies, safe to unit test in isolation.
    /// </summary>
    public static class MonthSummaryBuilder
    {
        private const decimal RecurringAmountTolerance = 0.10m;
        private const int TopMerchantCount = 3;
        public const string UncategorizedName = "Other";

        // Leading tokens that cores commonly prepend to card/ACH descriptions.
        private static readonly HashSet<string> DescriptionNoiseWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "POS", "DEBIT", "CARD", "CHECKCARD", "PURCHASE", "ACH", "WEB", "PMT", "WITHDRAWAL"
        };

        public static string DefaultInsightRulesJson =
            "[{\"id\":\"spend-up\",\"when\":\"percentChangeAtLeast\",\"value\":15,\"text\":\"Spending is up {percent}% compared to last month.\"}," +
            "{\"id\":\"spend-down\",\"when\":\"percentChangeAtMost\",\"value\":-15,\"text\":\"Nice work: spending is down {percent}% compared to last month.\"}," +
            "{\"id\":\"recurring\",\"when\":\"recurringAtLeast\",\"value\":3,\"text\":\"You have {recurringCount} recurring charges. Worth a quick review?\"}," +
            "{\"id\":\"top-merchant\",\"when\":\"topMerchantShareAtLeast\",\"value\":30,\"text\":\"{topMerchant} accounts for {topMerchantShare}% of your recent spending.\"}," +
            "{\"id\":\"steady\",\"when\":\"always\",\"value\":0,\"text\":\"Your spending is steady month over month.\"}]";

        public static MonthSummary Build(IEnumerable<TransactionInput> transactions, DateTime asOfUtc, string insightRulesJson = null, ICollection<string> dismissedInsightIds = null)
        {
            var spend = (transactions ?? Enumerable.Empty<TransactionInput>())
                .Where(t => t != null && t.IsDebit)
                .Select(t => new
                {
                    Amount = Math.Abs(t.Amount),
                    Name = NormalizeDescription(t.Description),
                    Category = string.IsNullOrWhiteSpace(t.Category) ? UncategorizedName : t.Category.Trim(),
                    MonthIndex = (t.PostingDate.Year * 12) + t.PostingDate.Month - 1
                })
                .ToList();

            var thisMonthIndex = (asOfUtc.Year * 12) + asOfUtc.Month - 1;

            var summary = new MonthSummary
            {
                ThisMonthSpend = spend.Where(t => t.MonthIndex == thisMonthIndex).Sum(t => t.Amount),
                LastMonthSpend = spend.Where(t => t.MonthIndex == thisMonthIndex - 1).Sum(t => t.Amount)
            };

            if (summary.LastMonthSpend > 0)
            {
                summary.PercentChange = Math.Round(((summary.ThisMonthSpend - summary.LastMonthSpend) / summary.LastMonthSpend) * 100m, 1);
            }

            var byMerchant = spend
                .Where(t => t.Name.Length > 0)
                .GroupBy(t => t.Name)
                .ToList();

            summary.TotalWindowSpend = spend.Sum(t => t.Amount);

            summary.TopMerchants = byMerchant
                .Select(g => new MerchantTotal
                {
                    Name = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Category = g.GroupBy(t => t.Category)
                        .OrderByDescending(cg => cg.Sum(t => t.Amount))
                        .First().Key
                })
                .OrderByDescending(m => m.Total)
                .ThenBy(m => m.Name)
                .Take(TopMerchantCount)
                .ToList();

            summary.ThisMonthCategories = spend
                .Where(t => t.MonthIndex == thisMonthIndex)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryTotal { Name = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(c => c.Total)
                .ThenBy(c => c.Name)
                .ToList();

            summary.LastMonthCategories = spend
                .Where(t => t.MonthIndex == thisMonthIndex - 1)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryTotal { Name = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(c => c.Total)
                .ThenBy(c => c.Name)
                .ToList();

            // Every category with any spend in the lookback window, carrying this month's spend
            // (zero when the spend was only in earlier months). Drives the budget editor.
            summary.AllCategories = spend
                .GroupBy(t => t.Category)
                .Select(g => new CategoryTotal { Name = g.Key, Total = g.Where(t => t.MonthIndex == thisMonthIndex).Sum(t => t.Amount) })
                .OrderByDescending(c => c.Total)
                .ThenBy(c => c.Name)
                .ToList();

            summary.RecurringCharges = byMerchant
                .Select(g =>
                {
                    var charge = FindRecurringCharge(g.Key, g.GroupBy(t => t.MonthIndex).ToDictionary(m => m.Key, m => m.Sum(t => t.Amount)));
                    if (charge != null)
                    {
                        charge.Category = g.GroupBy(t => t.Category)
                            .OrderByDescending(cg => cg.Sum(t => t.Amount))
                            .First().Key;
                    }
                    return charge;
                })
                .Where(r => r != null)
                .OrderByDescending(r => r.Amount)
                .ToList();

            ApplyInsight(summary, spend.Sum(t => t.Amount), insightRulesJson, dismissedInsightIds);
            return summary;
        }

        /// <summary>
        /// Uppercases, strips digits/punctuation and leading core noise words, and collapses whitespace,
        /// so "POS NETFLIX.COM #4821" and "NETFLIX COM" fold to the same merchant.
        /// </summary>
        public static string NormalizeDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(description.Length);
            foreach (var ch in description)
            {
                if (char.IsLetter(ch) || ch == '&')
                {
                    sb.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    sb.Append(' ');
                }
            }

            var words = sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (words.Count > 1 && DescriptionNoiseWords.Contains(words[0]))
            {
                words.RemoveAt(0);
            }

            return string.Join(" ", words.Take(4));
        }

        private static RecurringCharge FindRecurringCharge(string name, Dictionary<int, decimal> totalsByMonth)
        {
            foreach (var monthIndex in totalsByMonth.Keys.OrderByDescending(m => m))
            {
                decimal previous;
                if (!totalsByMonth.TryGetValue(monthIndex - 1, out previous))
                {
                    continue;
                }

                var current = totalsByMonth[monthIndex];
                var larger = Math.Max(current, previous);
                if (larger > 0 && Math.Abs(current - previous) <= RecurringAmountTolerance * larger)
                {
                    return new RecurringCharge { Name = name, Amount = current };
                }
            }

            return null;
        }

        private static void ApplyInsight(MonthSummary summary, decimal totalWindowSpend, string insightRulesJson, ICollection<string> dismissedInsightIds)
        {
            List<InsightRule> rules;
            try
            {
                rules = JsonConvert.DeserializeObject<List<InsightRule>>(
                    string.IsNullOrWhiteSpace(insightRulesJson) ? DefaultInsightRulesJson : insightRulesJson);
            }
            catch (JsonException)
            {
                rules = JsonConvert.DeserializeObject<List<InsightRule>>(DefaultInsightRulesJson);
            }

            var dismissed = dismissedInsightIds ?? new List<string>();
            var topMerchant = summary.TopMerchants.FirstOrDefault();
            decimal? topMerchantShare = null;
            if (topMerchant != null && totalWindowSpend > 0)
            {
                topMerchantShare = Math.Round((topMerchant.Total / totalWindowSpend) * 100m, 0);
            }

            foreach (var rule in (rules ?? new List<InsightRule>()).Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Text)))
            {
                if (dismissed.Contains(rule.Id) || !RuleMatches(rule, summary, topMerchantShare))
                {
                    continue;
                }

                summary.InsightId = rule.Id;
                summary.Insight = rule.Text
                    .Replace("{percent}", summary.PercentChange.HasValue ? Math.Abs(summary.PercentChange.Value).ToString("0.#", CultureInfo.InvariantCulture) : string.Empty)
                    .Replace("{recurringCount}", summary.RecurringCharges.Count.ToString(CultureInfo.InvariantCulture))
                    .Replace("{topMerchant}", topMerchant != null ? topMerchant.Name : string.Empty)
                    .Replace("{topMerchantShare}", topMerchantShare.HasValue ? topMerchantShare.Value.ToString("0", CultureInfo.InvariantCulture) : string.Empty);
                return;
            }
        }

        private static bool RuleMatches(InsightRule rule, MonthSummary summary, decimal? topMerchantShare)
        {
            switch ((rule.When ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "percentchangeatleast":
                    return summary.PercentChange.HasValue && summary.PercentChange.Value >= rule.Value;
                case "percentchangeatmost":
                    return summary.PercentChange.HasValue && summary.PercentChange.Value <= rule.Value;
                case "recurringatleast":
                    return summary.RecurringCharges.Count >= rule.Value;
                case "topmerchantshareatleast":
                    return topMerchantShare.HasValue && topMerchantShare.Value >= rule.Value;
                case "always":
                    return true;
                default:
                    return false;
            }
        }
    }
}
