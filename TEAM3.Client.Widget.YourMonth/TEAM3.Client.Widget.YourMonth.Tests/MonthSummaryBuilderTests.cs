using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TEAM3.Client.Widget.YourMonth.Models;

namespace TEAM3.Client.Widget.YourMonth.Tests
{
    [TestFixture]
    public class MonthSummaryBuilderTests
    {
        private static readonly DateTime AsOf = new DateTime(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

        private static TransactionInput Debit(decimal amount, string description, DateTime postingDate)
        {
            return new TransactionInput { Amount = amount, IsDebit = true, Description = description, PostingDate = postingDate };
        }

        private static TransactionInput Credit(decimal amount, string description, DateTime postingDate)
        {
            return new TransactionInput { Amount = amount, IsDebit = false, Description = description, PostingDate = postingDate };
        }

        [Test]
        public void Build_WithNoTransactions_ReturnsEmptySummary()
        {
            var summary = MonthSummaryBuilder.Build(new List<TransactionInput>(), AsOf);

            Assert.That(summary.ThisMonthSpend, Is.EqualTo(0m));
            Assert.That(summary.LastMonthSpend, Is.EqualTo(0m));
            Assert.That(summary.PercentChange, Is.Null);
            Assert.That(summary.TopMerchants, Is.Empty);
            Assert.That(summary.RecurringCharges, Is.Empty);
        }

        [Test]
        public void Build_SplitsSpendByCalendarMonth_AndIgnoresCredits()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(100m, "Grocery Mart", new DateTime(2026, 9, 5)),
                Debit(50m, "Gas Stop", new DateTime(2026, 9, 15)),
                Debit(200m, "Grocery Mart", new DateTime(2026, 8, 5)),
                Credit(1500m, "Payroll Deposit", new DateTime(2026, 9, 1)),
                Debit(75m, "Old Charge", new DateTime(2026, 7, 20))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.ThisMonthSpend, Is.EqualTo(150m));
            Assert.That(summary.LastMonthSpend, Is.EqualTo(200m));
        }

        [Test]
        public void Build_ComputesPercentChange_AgainstLastMonth()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(120m, "Store", new DateTime(2026, 9, 10)),
                Debit(100m, "Store", new DateTime(2026, 8, 10))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.PercentChange, Is.EqualTo(20m));
        }

        [Test]
        public void Build_WithNoSpendLastMonth_LeavesPercentChangeNull()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(120m, "Store", new DateTime(2026, 9, 10))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.PercentChange, Is.Null);
        }

        [Test]
        public void Build_TopMerchants_TakesTopThreeByTotal_WithNormalizedNames()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(40m, "POS NETFLIX.COM #123", new DateTime(2026, 9, 2)),
                Debit(40m, "NETFLIX COM", new DateTime(2026, 9, 20)),
                Debit(120m, "GROCERY MART STORE 42", new DateTime(2026, 9, 3)),
                Debit(60m, "GAS STOP", new DateTime(2026, 9, 4)),
                Debit(10m, "COFFEE HUT", new DateTime(2026, 9, 5))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.TopMerchants, Has.Count.EqualTo(3));
            Assert.That(summary.TopMerchants[0].Name, Is.EqualTo("GROCERY MART STORE"));
            Assert.That(summary.TopMerchants[0].Total, Is.EqualTo(120m));
            Assert.That(summary.TopMerchants[1].Name, Is.EqualTo("NETFLIX COM"));
            Assert.That(summary.TopMerchants[1].Total, Is.EqualTo(80m));
            Assert.That(summary.TopMerchants[1].Count, Is.EqualTo(2));
            Assert.That(summary.TopMerchants[2].Name, Is.EqualTo("GAS STOP"));
        }

        [Test]
        public void Build_TopMerchants_CarryDominantCategory_AndWindowTotal()
        {
            var transactions = new List<TransactionInput>
            {
                new TransactionInput { Amount = 100m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 9, 5) },
                new TransactionInput { Amount = 30m, IsDebit = true, Description = "Grocery Mart", Category = "Home Improvement", PostingDate = new DateTime(2026, 9, 6) },
                new TransactionInput { Amount = 40m, IsDebit = true, Description = "Gas Stop", Category = "Gas & Fuel", PostingDate = new DateTime(2026, 8, 15) },
                Credit(500m, "Payroll", new DateTime(2026, 9, 1))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.TopMerchants[0].Name, Is.EqualTo("GROCERY MART"));
            Assert.That(summary.TopMerchants[0].Category, Is.EqualTo("Groceries"));
            Assert.That(summary.TopMerchants[1].Category, Is.EqualTo("Gas & Fuel"));
            Assert.That(summary.TotalWindowSpend, Is.EqualTo(170m));
        }

        [Test]
        public void Build_RecurringCharges_RequiresConsecutiveMonthsWithinTenPercent()
        {
            var transactions = new List<TransactionInput>
            {
                // Recurring: same merchant, consecutive months, within 10%
                Debit(15.49m, "NETFLIX.COM", new DateTime(2026, 8, 12)),
                Debit(15.99m, "NETFLIX.COM", new DateTime(2026, 9, 12)),
                // Not recurring: amounts differ by far more than 10%
                Debit(20m, "GROCERY MART", new DateTime(2026, 8, 1)),
                Debit(200m, "GROCERY MART", new DateTime(2026, 9, 1)),
                // Not recurring: single month only
                Debit(9.99m, "ONE TIME SHOP", new DateTime(2026, 9, 7))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.RecurringCharges, Has.Count.EqualTo(1));
            Assert.That(summary.RecurringCharges[0].Name, Is.EqualTo("NETFLIX COM"));
            Assert.That(summary.RecurringCharges[0].Amount, Is.EqualTo(15.99m));
            Assert.That(summary.RecurringCharges[0].Category, Is.EqualTo(MonthSummaryBuilder.UncategorizedName));
        }

        [Test]
        public void Build_RecurringCharges_SkipsNonConsecutiveMonths()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(12m, "GYM MEMBERSHIP", new DateTime(2026, 7, 3)),
                Debit(12m, "GYM MEMBERSHIP", new DateTime(2026, 9, 3))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.RecurringCharges, Is.Empty);
        }

        [Test]
        public void Build_Insight_PicksFirstMatchingRule_AndFillsTokens()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(150m, "Store", new DateTime(2026, 9, 10)),
                Debit(100m, "Store", new DateTime(2026, 8, 10))
            };
            var rules = "[{\"id\":\"spend-up\",\"when\":\"percentChangeAtLeast\",\"value\":20,\"text\":\"Up {percent}% vs last month.\"}," +
                        "{\"id\":\"steady\",\"when\":\"always\",\"value\":0,\"text\":\"Steady.\"}]";

            var summary = MonthSummaryBuilder.Build(transactions, AsOf, rules);

            Assert.That(summary.InsightId, Is.EqualTo("spend-up"));
            Assert.That(summary.Insight, Is.EqualTo("Up 50% vs last month."));
        }

        [Test]
        public void Build_Insight_SkipsDismissedRules()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(150m, "Store", new DateTime(2026, 9, 10)),
                Debit(100m, "Store", new DateTime(2026, 8, 10))
            };
            var rules = "[{\"id\":\"spend-up\",\"when\":\"percentChangeAtLeast\",\"value\":20,\"text\":\"Up.\"}," +
                        "{\"id\":\"steady\",\"when\":\"always\",\"value\":0,\"text\":\"Steady.\"}]";

            var summary = MonthSummaryBuilder.Build(transactions, AsOf, rules, new List<string> { "spend-up" });

            Assert.That(summary.InsightId, Is.EqualTo("steady"));
        }

        [Test]
        public void Build_Insight_FallsBackToDefaultRules_OnInvalidJson()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(10m, "Store", new DateTime(2026, 9, 10)),
                Debit(10m, "Store", new DateTime(2026, 8, 10))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf, "{ not valid json ]");

            Assert.That(summary.InsightId, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Build_Insight_WhenAllRulesDismissed_LeavesInsightNull()
        {
            var transactions = new List<TransactionInput> { Debit(10m, "Store", new DateTime(2026, 9, 10)) };
            var rules = "[{\"id\":\"steady\",\"when\":\"always\",\"value\":0,\"text\":\"Steady.\"}]";

            var summary = MonthSummaryBuilder.Build(transactions, AsOf, rules, new List<string> { "steady" });

            Assert.That(summary.Insight, Is.Null);
            Assert.That(summary.InsightId, Is.Null);
        }

        [Test]
        public void Build_CategoryTotals_GroupBySpendPerCalendarMonth()
        {
            var transactions = new List<TransactionInput>
            {
                new TransactionInput { Amount = 100m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 9, 5) },
                new TransactionInput { Amount = 40m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 9, 12) },
                new TransactionInput { Amount = 50m, IsDebit = true, Description = "Gas Stop", Category = "Gas & Fuel", PostingDate = new DateTime(2026, 9, 15) },
                new TransactionInput { Amount = 80m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 8, 5) },
                new TransactionInput { Amount = 2000m, IsDebit = false, Description = "Payroll", Category = "Paycheck", PostingDate = new DateTime(2026, 9, 1) }
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.ThisMonthCategories, Has.Count.EqualTo(2));
            Assert.That(summary.ThisMonthCategories[0].Name, Is.EqualTo("Groceries"));
            Assert.That(summary.ThisMonthCategories[0].Total, Is.EqualTo(140m));
            Assert.That(summary.ThisMonthCategories[1].Name, Is.EqualTo("Gas & Fuel"));
            Assert.That(summary.LastMonthCategories, Has.Count.EqualTo(1));
            Assert.That(summary.LastMonthCategories[0].Total, Is.EqualTo(80m));
        }

        [Test]
        public void Build_CategoryTotals_BucketUncategorizedAsOther()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(25m, "Mystery Store", new DateTime(2026, 9, 5)),
                new TransactionInput { Amount = 10m, IsDebit = true, Description = "Shop", Category = "  ", PostingDate = new DateTime(2026, 9, 6) }
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.ThisMonthCategories, Has.Count.EqualTo(1));
            Assert.That(summary.ThisMonthCategories[0].Name, Is.EqualTo(MonthSummaryBuilder.UncategorizedName));
            Assert.That(summary.ThisMonthCategories[0].Total, Is.EqualTo(35m));
        }

        [Test]
        public void Build_CategoryTotals_EmptyMonthsProduceEmptyLists()
        {
            var summary = MonthSummaryBuilder.Build(new List<TransactionInput>(), AsOf);

            Assert.That(summary.ThisMonthCategories, Is.Empty);
            Assert.That(summary.LastMonthCategories, Is.Empty);
        }

        [Test]
        public void CategoryBudgetHelper_ParsesAndSanitizes()
        {
            var json = "{\"Groceries\":450.005,\" Gas & Fuel \":100,\"Bad\":-5,\"TooBig\":2000000,\"\":10}";

            var budgets = CategoryBudgetHelper.Parse(json);

            Assert.That(budgets, Has.Count.EqualTo(2));
            Assert.That(budgets["Groceries"], Is.EqualTo(450.00m).Or.EqualTo(450.01m));
            Assert.That(budgets["Gas & Fuel"], Is.EqualTo(100m));
        }

        [Test]
        public void CategoryBudgetHelper_InvalidJsonReturnsEmpty()
        {
            Assert.That(CategoryBudgetHelper.Parse("{ not json ]"), Is.Empty);
            Assert.That(CategoryBudgetHelper.Parse(null), Is.Empty);
            Assert.That(CategoryBudgetHelper.Parse(""), Is.Empty);
        }

        [Test]
        public void CategoryBudgetHelper_RoundTripsThroughSerialize()
        {
            var budgets = new Dictionary<string, decimal> { { "Groceries", 450m }, { "Coffee Shops", 60.50m } };

            var roundTripped = CategoryBudgetHelper.Parse(CategoryBudgetHelper.Serialize(budgets));

            Assert.That(roundTripped, Has.Count.EqualTo(2));
            Assert.That(roundTripped["Coffee Shops"], Is.EqualTo(60.50m));
        }

        [Test]
        public void Build_AllCategories_CoversWholeWindowWithThisMonthSpend()
        {
            var transactions = new List<TransactionInput>
            {
                new TransactionInput { Amount = 100m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 9, 5) },
                new TransactionInput { Amount = 80m, IsDebit = true, Description = "Grocery Mart", Category = "Groceries", PostingDate = new DateTime(2026, 8, 5) },
                // Spend only in an earlier month: still listed, with zero this-month spend
                new TransactionInput { Amount = 60m, IsDebit = true, Description = "Shoe Warehouse", Category = "Clothing", PostingDate = new DateTime(2026, 8, 10) }
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.AllCategories, Has.Count.EqualTo(2));
            Assert.That(summary.AllCategories[0].Name, Is.EqualTo("Groceries"));
            Assert.That(summary.AllCategories[0].Total, Is.EqualTo(100m));
            Assert.That(summary.AllCategories[1].Name, Is.EqualTo("Clothing"));
            Assert.That(summary.AllCategories[1].Total, Is.EqualTo(0m));
        }

        [Test]
        public void CategoryBudgetHelper_NameList_SanitizesAndRoundTrips()
        {
            var parsed = CategoryBudgetHelper.ParseNameList("[\" Clothing \",\"\",\"clothing\",\"Gas & Fuel\"]");

            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Does.Contain("Clothing"));
            Assert.That(parsed, Does.Contain("Gas & Fuel"));
            Assert.That(CategoryBudgetHelper.ParseNameList("{ not a list }"), Is.Empty);
            Assert.That(CategoryBudgetHelper.ParseNameList(CategoryBudgetHelper.SerializeNameList(parsed)), Is.EqualTo(parsed));
        }

        [Test]
        public void NormalizeDescription_StripsNoiseDigitsAndPunctuation()
        {
            Assert.That(MonthSummaryBuilder.NormalizeDescription("POS NETFLIX.COM #4821"), Is.EqualTo("NETFLIX COM"));
            Assert.That(MonthSummaryBuilder.NormalizeDescription("  debit CARD grocery-mart 09/12 "), Is.EqualTo("GROCERY MART"));
            Assert.That(MonthSummaryBuilder.NormalizeDescription(null), Is.EqualTo(string.Empty));
            Assert.That(MonthSummaryBuilder.NormalizeDescription("12345"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Build_IgnoresVoidLikeZeroAmounts_InRecurringDetection()
        {
            var transactions = new List<TransactionInput>
            {
                Debit(0m, "ZERO CHARGE", new DateTime(2026, 8, 1)),
                Debit(0m, "ZERO CHARGE", new DateTime(2026, 9, 1))
            };

            var summary = MonthSummaryBuilder.Build(transactions, AsOf);

            Assert.That(summary.RecurringCharges, Is.Empty);
        }
    }
}
