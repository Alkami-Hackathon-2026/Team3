using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Alkami.Client.Framework.Mvc;
using Alkami.Client.Framework.Utility;
using Alkami.Common;
using Alkami.MicroServices.Security.Contracts;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.MicroServices.Security.Data;
using Alkami.MicroServices.Security.Service.Client;
using Alkami.MicroServices.Transactions.Contracts;
using Alkami.MicroServices.Transactions.Contracts.Requests;
using Alkami.MicroServices.Transactions.Service.Client;
using Alkami.Security.Common.Claims;
using Common.Logging;
using TEAM3.Client.Widget.YourMonth.Models;
using WebToolkit;
using Transaction = Alkami.MicroServices.Transactions.Data.Transaction;

namespace TEAM3.Client.Widget.YourMonth.Controllers
{
    [ClaimsAuthorizationFilter(PermissionNames.NoPermissions)]
    public class TEAM3YourMonthController : BaseController
    {
        private const string WidgetName = "TEAM3YourMonth";
        private const string SettingKeyLookbackDays = "LookbackDays";
        private const string SettingKeyInsightRules = "InsightRules";
        private const string SettingKeyDismissedInsights = "DismissedInsights";
        private const int DefaultLookbackDays = 60;
        private const int TransactionPageSize = 500;
        private const int MaxTransactionPages = 40;

        private static readonly ILog Logger = LogManager.GetLogger<TEAM3YourMonthController>();

        public static Func<ISecurityServiceContract> SecurityServiceFactory = () => new SecurityServiceClient();
        public static Func<ITransactionServiceContract> TransactionServiceFactory = () => new TransactionServiceClient();

        private readonly string logPrefix = WidgetName;

        public ActionResult Index()
        {
            try
            {
                Logger.DebugFormat("[{0}] [GET] Controller/Index", logPrefix);

                var model = new TEAM3YourMonthModel { FirstName = CurrentUser.FirstName };
                return View("Index", model);
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("Error");
            }
        }

        [HttpGet]
        public JsonResult Summary()
        {
            try
            {
                var lookbackDays = GetLookbackDays();
                var accountIds = GetOwnedAccountIds();
                var since = DateTime.UtcNow.Date.AddDays(-lookbackDays);
                var transactions = GetTransactions(accountIds, since);

                var inputs = transactions
                    .Where(t => t != null && !t.IsVoid)
                    .Select(t => new TransactionInput
                    {
                        Amount = t.Amount,
                        IsDebit = t.Debit,
                        Description = string.IsNullOrWhiteSpace(t.SpecificDescription) ? t.GeneralDescription : t.SpecificDescription,
                        PostingDate = t.PostingDate
                    })
                    .ToList();

                var summary = MonthSummaryBuilder.Build(inputs, DateTime.UtcNow, GetInsightRulesJson(), GetDismissedInsightIds());

                Logger.DebugFormat("[{0}] Summary built from [{1}] transactions across [{2}] accounts", logPrefix, inputs.Count, accountIds.Count);
                return Json(summary, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Summary", e);
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                return Json(new { errorMessage = "We could not load your month right now. Please try again later." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DismissInsight(string insightId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(insightId) || insightId.Length > 100 || insightId.Contains(","))
                {
                    Response.StatusCode = 400;
                    Response.TrySkipIisCustomErrors = true;
                    return Json(new { errorMessage = "Invalid insight identifier." });
                }

                var dismissed = GetDismissedInsightIds();
                if (!dismissed.Contains(insightId))
                {
                    dismissed.Add(insightId);
                    SaveDismissedInsightIds(dismissed);
                }

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error("Error [POST] Controller/DismissInsight", e);
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                return Json(new { errorMessage = "We could not save your preference right now." });
            }
        }

        /// <summary>
        /// Accounts the member owns and can see: not deleted, not hidden by the FI or the member,
        /// and core-owned rather than linked through cross-account or shared access.
        /// </summary>
        private List<long> GetOwnedAccountIds()
        {
            var request = new GetUserRequest
            {
                UserId = CurrentUser.Id,
                Mapping = new UserMapper { ShouldIncludeUserAccounts = true }
            };
            this.AugmentRequest(request);

            var response = AsyncHelper.RunSync(() => SecurityServiceFactory().GetUserAsync(request));
            if (response == null || response.HasError)
            {
                throw new InvalidOperationException("Security service returned an error while loading user accounts.");
            }

            var user = response.Users != null ? response.Users.FirstOrDefault() : null;
            if (user == null || user.IsDeleted || user.UserAccounts == null)
            {
                return new List<long>();
            }

            return user.UserAccounts
                .Where(ua => ua != null
                    && !ua.Deleted
                    && !ua.HideFromEndUser
                    && !ua.HiddenByEndUser
                    && (ua.RelationshipSubType == null || ua.RelationshipSubType == RelationshipSubType.CoreLinked))
                .Select(ua => ua.AccountId)
                .Distinct()
                .ToList();
        }

        private List<Transaction> GetTransactions(List<long> accountIds, DateTime since)
        {
            var results = new List<Transaction>();
            if (accountIds == null || accountIds.Count == 0)
            {
                return results;
            }

            var request = new GetTransactionsRequest
            {
                Filter = new TransactionFilter
                {
                    AccountIds = accountIds,
                    StartDate = since,
                    DateSearchField = TransactionDateFields.PostingDate
                },
                MaxResults = TransactionPageSize
            };
            this.AugmentRequest(request);

            var page = 0;
            Alkami.MicroServices.Transactions.Contracts.Responses.TransactionResponse response;
            do
            {
                request.Page = page;
                response = AsyncHelper.RunSync(() => TransactionServiceFactory().GetTransactionsAsync(request));
                if (response == null || response.HasError)
                {
                    throw new InvalidOperationException("Transactions service returned an error while loading transactions.");
                }

                if (response.Transactions != null && response.Transactions.Count > 0)
                {
                    results.AddRange(response.Transactions);
                }

                page++;
            } while (response.Transactions != null
                && response.Transactions.Count > 0
                && results.Count < response.TotalResults
                && page < MaxTransactionPages);

            return results;
        }

        private int GetLookbackDays()
        {
            var raw = WidgetSettingsUtil.GetStringValue(UserWidgetSettings, SettingKeyLookbackDays);
            int days;
            if (!int.TryParse(raw, out days) || days < 1 || days > 366)
            {
                return DefaultLookbackDays;
            }

            return days;
        }

        private string GetInsightRulesJson()
        {
            return WidgetSettingsUtil.GetStringValue(UserWidgetSettings, SettingKeyInsightRules);
        }

        private List<string> GetDismissedInsightIds()
        {
            var raw = WidgetSettingsUtil.GetStringValue(UserWidgetSettings, SettingKeyDismissedInsights);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<string>();
            }

            return raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }

        private void SaveDismissedInsightIds(List<string> dismissedIds)
        {
            var userWidgets = WidgetRepository.GetUserWidgets();
            var userWidget = userWidgets.Model.FirstOrDefault(x => x.Widget.Name == WidgetName);
            var widgetId = userWidget != null ? userWidget.Widget.Id : 0L;
            if (widgetId <= 0)
            {
                throw new InvalidOperationException("Widget registration row was not found; cannot save user widget settings.");
            }

            var settings = new Dictionary<string, string>
            {
                { SettingKeyDismissedInsights, string.Join(",", dismissedIds) }
            };
            UpdateUserWidgetSettings(widgetId, settings);
        }
    }
}
