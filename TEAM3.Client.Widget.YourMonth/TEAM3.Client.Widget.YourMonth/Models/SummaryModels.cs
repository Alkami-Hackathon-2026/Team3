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

        public string Category { get; set; }
    }

    public class CategoryTotal
    {
        public string Name { get; set; }

        public decimal Total { get; set; }
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

        public List<CategoryTotal> ThisMonthCategories { get; set; } = new List<CategoryTotal>();

        public List<CategoryTotal> LastMonthCategories { get; set; } = new List<CategoryTotal>();

        /// <summary>Every category with spend anywhere in the lookback window; Total is this month's spend.</summary>
        public List<CategoryTotal> AllCategories { get; set; } = new List<CategoryTotal>();

        /// <summary>Per-user monthly budget by category name; populated by the controller from user widget settings.</summary>
        public Dictionary<string, decimal> CategoryBudgets { get; set; } = new Dictionary<string, decimal>();

        /// <summary>Categories the user removed from the budget editor; populated by the controller from user widget settings.</summary>
        public List<string> RemovedBudgetCategories { get; set; } = new List<string>();

        /// <summary>All expense categories in the user's TransactionCategory tree; populated by the controller.</summary>
        public List<string> ExpenseCategories { get; set; } = new List<string>();

        public string InsightId { get; set; }

        public string Insight { get; set; }
    }
}
