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
