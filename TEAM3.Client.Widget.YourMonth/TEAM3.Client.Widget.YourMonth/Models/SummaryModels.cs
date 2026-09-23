using System;
using System.Collections.Generic;

namespace TEAM3.Client.Widget.YourMonth.Models
{
    /// <summary>
    /// Minimal transaction shape consumed by MonthSummaryBuilder.
    /// Kept free of Alkami types so the builder and its tests have no platform dependencies.
    /// </summary>
    public class TransactionInput
    {
        public decimal Amount { get; set; }

        public bool IsDebit { get; set; }

        public string Description { get; set; }

        public DateTime PostingDate { get; set; }
    }

    public class MerchantTotal
    {
        public string Name { get; set; }

        public decimal Total { get; set; }

        public int Count { get; set; }
    }

    public class RecurringCharge
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }
    }

    public class InsightRule
    {
        public string Id { get; set; }

        public string When { get; set; }

        public decimal Value { get; set; }

        public string Text { get; set; }
    }

    public class MonthSummary
    {
        public decimal ThisMonthSpend { get; set; }

        public decimal LastMonthSpend { get; set; }

        /// <summary>Percent change of this month vs last month, null when last month had no spend.</summary>
        public decimal? PercentChange { get; set; }

        public List<MerchantTotal> TopMerchants { get; set; } = new List<MerchantTotal>();

        public List<RecurringCharge> RecurringCharges { get; set; } = new List<RecurringCharge>();

        public string InsightId { get; set; }

        public string Insight { get; set; }
    }
}
