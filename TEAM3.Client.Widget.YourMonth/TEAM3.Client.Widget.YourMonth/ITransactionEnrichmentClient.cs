// OpenApiTool v1.6.1.0
//
// NOTE: This client is auto-generated and should not be manually modified.
// Any additions should be made in an adjacent partial interface.
//
using Alkami.Utilities.ServiceAttributes;
using Refit;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

[assembly: ServiceReference("transaction-enrichment", 1, 14)]

namespace Alkami.Services.TransactionEnrichment.RefitClient
{
    /// <summary>
    /// TransactionEnrichment Service v.1.14 - Api for transactions
    /// </summary>
    [ServiceClient("transaction-enrichment", 1)]
    public partial interface ITransactionEnrichmentClient
    {
        /// <summary>
        /// Gets the list of transaction categories available.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionCategoryResponse[]}"/> from the operation.</returns>
        [Get("/admin/v1/users/{userIdentifier}/transaction-categories")]
        Task<IApiResponse<TransactionCategoryResponse[]>> GetAdminV1UsersTransactionCategoriesAsync([Required] Guid userIdentifier, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets categories on a transaction.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="transactionCategoryUpdateRequest">The category splits for the transaction.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="HttpResponseMessage"/> from the operation.</returns>
        [Put("/admin/v1/users/{userIdentifier}/accounts/{accountIdentifier}/transactions/{transactionId}/categories")]
        Task<HttpResponseMessage> PutAdminV1UsersAccountsTransactionsCategoriesAsync([Required] Guid userIdentifier, [Required] Guid accountIdentifier, [Required] long transactionId, [Body] TransactionCategoryUpdateRequest transactionCategoryUpdateRequest = default(TransactionCategoryUpdateRequest), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get transactions for an account as admin. Pending transactions will be returned first, sorted by PostedDate.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="pageIndex">Page index. Default is 0.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="startDateTime">Start time for use in retrieval of elements (ISO 8601). Default is 90 days before today at midnight.</param>
        /// <param name="endDateTime">End time for use in retrieval of elements (ISO 8601). Default is today 23:59.</param>
        /// <param name="orderBy">Comma-separated list of {Field} {Direction} pairs. <br> Field must be one of: DisplayDate, TransactionType, Amount, GeneralDescription, SpecificDescription, TransactionId, Balance. <br> Direction: asc|ascending|desc|descending. Defaults to ascending.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionResponseApiPagedResponse}"/> from the operation.</returns>
        [Get("/admin/v1/users/{userIdentifier}/accounts/{accountIdentifier}/transactions")]
        Task<IApiResponse<TransactionResponseApiPagedResponse>> GetAdminV1UsersAccountsTransactionsAsync([Required] Guid userIdentifier, [Required] Guid accountIdentifier, int? pageIndex = default(int?), int? pageSize = default(int?), DateTime? startDateTime = default(DateTime?), DateTime? endDateTime = default(DateTime?), string orderBy = default(string), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a search for transactions.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionSearchRequest">Transaction search request</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionResponseApiPagedResponse}"/> from the operation.</returns>
        [Post("/admin/v1/users/{userIdentifier}/accounts/{accountIdentifier}/transactions-search")]
        Task<IApiResponse<TransactionResponseApiPagedResponse>> PostAdminV1UsersAccountsTransactionsSearchAsync([Required] Guid userIdentifier, [Required] Guid accountIdentifier, [Body] TransactionSearchRequest transactionSearchRequest = default(TransactionSearchRequest), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets details for a single transaction.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionDetailResponse}"/> from the operation.</returns>
        [Get("/admin/v1/users/{userIdentifier}/accounts/{accountIdentifier}/transactions/{transactionId}")]
        Task<IApiResponse<TransactionDetailResponse>> GetAdminV1UsersAccountsTransactionsByUserIdentifierAsync([Required] Guid userIdentifier, [Required] Guid accountIdentifier, [Required] long transactionId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets the memo on a transaction.
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="memoUpdateRequest">The memo to set on the transaction.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="HttpResponseMessage"/> from the operation.</returns>
        [Put("/admin/v1/users/{userIdentifier}/accounts/{accountIdentifier}/transactions/{transactionId}/memo")]
        Task<HttpResponseMessage> PutAdminV1UsersAccountsTransactionsMemoAsync([Required] Guid userIdentifier, [Required] Guid accountIdentifier, [Required] long transactionId, [Body] MemoUpdateRequest memoUpdateRequest = default(MemoUpdateRequest), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the list of transaction categories available.
        /// </summary>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionCategoryResponse[]}"/> from the operation.</returns>
        [Get("/afx/v2/transaction-categories")]
        Task<IApiResponse<TransactionCategoryResponse[]>> GetAfxV2TransactionCategoriesAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets categories on a transaction.
        /// </summary>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="transactionCategoryUpdateRequest">The category splits for the transaction.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="HttpResponseMessage"/> from the operation.</returns>
        [Put("/afx/v2/accounts/{accountIdentifier}/transactions/{transactionId}/categories")]
        Task<HttpResponseMessage> PutAfxV2AccountsTransactionsCategoriesAsync([Required] Guid accountIdentifier, [Required] long transactionId, [Body] TransactionCategoryUpdateRequest transactionCategoryUpdateRequest = default(TransactionCategoryUpdateRequest), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get transactions for an account. Pending transactions will be returned first, sorted by PostedDate.
        /// </summary>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="pageIndex">Page index. Default is 0.</param>
        /// <param name="pageSize">Page size. Default is 100.</param>
        /// <param name="startDateTime">Start time for use in retrieval of elements (ISO 8601). Default is 90 days before today at midnight.</param>
        /// <param name="endDateTime">End time for use in retrieval of elements (ISO 8601). Default is today 23:59.</param>
        /// <param name="orderBy">Comma-separated list of {Field} {Direction} pairs. <br> Field must be one of: DisplayDate, TransactionType, Amount, GeneralDescription, SpecificDescription, TransactionId, Balance. <br> Direction: asc|ascending|desc|descending. Defaults to ascending.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionResponseApiPagedResponse}"/> from the operation.</returns>
        [Get("/afx/v2/accounts/{accountIdentifier}/transactions")]
        Task<IApiResponse<TransactionResponseApiPagedResponse>> GetAfxV2AccountsTransactionsAsync([Required] Guid accountIdentifier, int? pageIndex = default(int?), int? pageSize = default(int?), DateTime? startDateTime = default(DateTime?), DateTime? endDateTime = default(DateTime?), string orderBy = default(string), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a search for transactions.
        /// </summary>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionSearchRequest">Transaction search request</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionResponseApiPagedResponse}"/> from the operation.</returns>
        [Post("/afx/v2/accounts/{accountIdentifier}/transactions-search")]
        Task<IApiResponse<TransactionResponseApiPagedResponse>> PostAfxV2AccountsTransactionsSearchAsync([Required] Guid accountIdentifier, [Body] TransactionSearchRequest transactionSearchRequest = default(TransactionSearchRequest), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets details for a single transaction.
        /// </summary>
        /// <param name="accountIdentifier"></param>
        /// <param name="transactionId"></param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="IApiResponse{TransactionDetailResponse}"/> from the operation.</returns>
        [Get("/afx/v2/accounts/{accountIdentifier}/transactions/{transactionId}")]
        Task<IApiResponse<TransactionDetailResponse>> GetAfxV2AccountsTransactionsByAccountIdentifierAsync([Required] Guid accountIdentifier, [Required] long transactionId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets the memo on a transaction.
        /// </summary>
        /// <param name="accountIdentifier">Account identifier</param>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="memoUpdateRequest">The memo to set on the transaction.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task of <see cref="HttpResponseMessage"/> from the operation.</returns>
        [Put("/afx/v2/accounts/{accountIdentifier}/transactions/{transactionId}/memo")]
        Task<HttpResponseMessage> PutAfxV2AccountsTransactionsMemoAsync([Required] Guid accountIdentifier, [Required] long transactionId, [Body] MemoUpdateRequest memoUpdateRequest = default(MemoUpdateRequest), CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Transaction category response
    /// </summary>
    public class TransactionCategoryResponse
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sub categories
        /// </summary>
        public TransactionSubCategory[] Sub_categories { get; set; }
    }

    /// <summary>
    /// Transaction category update request.
    /// </summary>
    public class TransactionCategoryUpdateRequest
    {
        /// <summary>
        /// Transaction category updates
        /// </summary>
        public TransactionCategoryUpdate[] TransactionCategoryUpdates { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class TransactionResponseApiPagedResponse
    {
        /// <summary>
        ///
        /// </summary>
        public TransactionResponse[] Results { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Transaction search request
    /// </summary>
    public class TransactionSearchRequest
    {
        /// <summary>
        /// Amount minimum.
        /// </summary>
        public double? AmountMinimum { get; set; }

        /// <summary>
        /// Amount maximum.
        /// </summary>
        public double? AmountMaximum { get; set; }

        /// <summary>
        /// A comma-separated list of categories.
        /// </summary>
        public string[] Categories { get; set; }

        /// <summary>
        /// The start check number to search for.
        /// </summary>
        public long? StartCheckNumber { get; set; }

        /// <summary>
        /// The end check number to search for. Must be larger than startCheckNumber.
        /// </summary>
        public long? EndCheckNumber { get; set; }

        /// <summary>
        /// Whether to search for a check .
        /// </summary>
        public bool? IsCheck { get; set; }

        /// <summary>
        /// The search string to use to filter by description fields.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string TransactionStatus { get; set; }

        /// <summary>
        /// Page index. Defaults to 0.
        /// </summary>
        public int? PageIndex { get; set; }

        /// <summary>
        /// Page size. Defaults to 50.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Start time for use in retrieval of elements (ISO 8601). Default is 90 days before today at midnight. Difference between startDateTime and endDateTime cannot be larger than 90 days.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// End time for use in retrieval of elements (ISO 8601). Default is today 23:59. Difference between startDateTime and endDateTime cannot be larger than 90 days.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// OrderByDirection
        /// </summary>
        public string OrderByDirection { get; set; }

        /// <summary>
        /// Field to sort the transactions by. Overrides institution configured sorting.
        /// </summary>
        public string SortField { get; set; }
    }

    /// <summary>
    /// Detailed transaction response
    /// </summary>
    public class TransactionDetailResponse
    {
        /// <summary>
        /// The transaction ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The date
        /// </summary>
        public DateTime? DisplayDateTime { get; set; }

        /// <summary>
        /// The account the transaction is associated with
        /// </summary>
        public Guid AccountIdentifier { get; set; }

        /// <summary>
        /// The amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The balance at the time of the transaction
        /// </summary>
        public double? RunningBalance { get; set; }

        /// <summary>
        /// The original description of the transaction
        /// </summary>
        public string GeneralDescription { get; set; }

        /// <summary>
        /// The cleansed description of the transaction
        /// </summary>
        public string SpecificDescription { get; set; }

        /// <summary>
        /// The check number associated with the transaction if it exists
        /// </summary>
        public long? CheckNumber { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Detail fields
        /// </summary>
        public DetailField[] DetailFields { get; set; }

        /// <summary>
        /// Dispute link
        /// </summary>
        public string DisputeLink { get; set; }

        /// <summary>
        /// Mobile dispute link
        /// </summary>
        public string MobileDisputeLink { get; set; }

        /// <summary>
        /// Single sign on URL
        /// </summary>
        public string SsoUrl { get; set; }

        /// <summary>
        /// split-categories
        /// </summary>
        public TransactionCategorySplitResponse[] Categories { get; set; }
    }

    /// <summary>
    /// Memo update request.
    /// </summary>
    public class MemoUpdateRequest
    {
        /// <summary>
        /// Memo.
        /// </summary>
        public string Memo { get; set; }
    }

    /// <summary>
    /// Transaction sub category
    /// </summary>
    public class TransactionSubCategory
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Transaction category update
    /// </summary>
    public class TransactionCategoryUpdate
    {
        /// <summary>
        /// Transaction category ID
        /// </summary>
        public long TransactionCategoryId { get; set; }

        /// <summary>
        /// Amount for this transaction category
        /// </summary>
        public double Amount { get; set; }
    }

    /// <summary>
    /// Transaction
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>
        /// The transaction ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The date
        /// </summary>
        public DateTime? DisplayDateTime { get; set; }

        /// <summary>
        /// The account the transaction is associated with
        /// </summary>
        public Guid AccountIdentifier { get; set; }

        /// <summary>
        /// The amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The balance at the time of the transaction
        /// </summary>
        public double? RunningBalance { get; set; }

        /// <summary>
        /// The original description of the transaction
        /// </summary>
        public string GeneralDescription { get; set; }

        /// <summary>
        /// The cleansed description of the transaction
        /// </summary>
        public string SpecificDescription { get; set; }

        /// <summary>
        /// The check number associated with the transaction if it exists
        /// </summary>
        public long? CheckNumber { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Transaction type.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The list of categories for the transaction
        /// </summary>
        public string[] Categories { get; set; }
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class TransactionTypeValues
    {
        /// <summary>
        /// Credit
        /// </summary>
        public const string Credit = "Credit";

        /// <summary>
        /// Debit
        /// </summary>
        public const string Debit = "Debit";
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class TransactionStatusValues
    {
        /// <summary>
        /// Pending
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// Posted
        /// </summary>
        public const string Posted = "Posted";
    }

    /// <summary>
    /// OrderByDirection
    /// </summary>
    public static class OrderByDirectionValues
    {
        /// <summary>
        /// Ascending
        /// </summary>
        public const string Ascending = "Ascending";

        /// <summary>
        /// Descending
        /// </summary>
        public const string Descending = "Descending";
    }

    /// <summary>
    /// Field to sort the transactions by. Overrides institution configured sorting.
    /// </summary>
    public static class SortFieldValues
    {
        /// <summary>
        /// DisplayDate
        /// </summary>
        public const string DisplayDate = "DisplayDate";

        /// <summary>
        /// TransactionType
        /// </summary>
        public const string TransactionType = "TransactionType";

        /// <summary>
        /// Amount
        /// </summary>
        public const string Amount = "Amount";

        /// <summary>
        /// GeneralDescription
        /// </summary>
        public const string GeneralDescription = "GeneralDescription";

        /// <summary>
        /// SpecificDescription
        /// </summary>
        public const string SpecificDescription = "SpecificDescription";

        /// <summary>
        /// TransactionId
        /// </summary>
        public const string TransactionId = "TransactionId";

        /// <summary>
        /// Balance
        /// </summary>
        public const string Balance = "Balance";
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class TypeValues
    {
        /// <summary>
        /// Credit
        /// </summary>
        public const string Credit = "Credit";

        /// <summary>
        /// Debit
        /// </summary>
        public const string Debit = "Debit";
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class StatusValues
    {
        /// <summary>
        /// Pending
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// Posted
        /// </summary>
        public const string Posted = "Posted";
    }

    /// <summary>
    /// Detail field.
    /// </summary>
    public class DetailField
    {
        /// <summary>
        /// Detail field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detail field display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Detail field value
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Transaction category split by amount
    /// </summary>
    public class TransactionCategorySplitResponse
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public double Amount { get; set; }
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class TypeValues1
    {
        /// <summary>
        /// Credit
        /// </summary>
        public const string Credit = "Credit";

        /// <summary>
        /// Debit
        /// </summary>
        public const string Debit = "Debit";
    }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public static class StatusValues1
    {
        /// <summary>
        /// Pending
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// Posted
        /// </summary>
        public const string Posted = "Posted";
    }
}