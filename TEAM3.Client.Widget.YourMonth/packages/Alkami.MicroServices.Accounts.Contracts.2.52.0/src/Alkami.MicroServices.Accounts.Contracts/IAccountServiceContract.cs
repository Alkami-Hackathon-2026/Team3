using Alkami.MicroServices.Accounts.Contracts.Requests;
using Alkami.MicroServices.Accounts.Contracts.Responses;
using Alkami.Security;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Accounts.Contracts
{
    /// <summary>
    /// Account Service Contract
    /// </summary>
    [ServiceContract]
    public interface IAccountServiceContract
    {
        /// <summary>
        /// Account Service request to create or modify one or many Accounts
        /// </summary>
        /// <param cref="AddOrUpdateAccountRequest" name="request"></param>
        /// <returns cref="AccountResponse"></returns>
        [OperationContract]
        Task<AccountResponse> AddOrUpdateAccountAsync(AddOrUpdateAccountRequest request);

        /// <summary>
        /// Account Service request to create or modify one or many Account Types
        /// </summary>
        /// <param cref="AddOrUpdateAccountTypeRequest" name="request"></param>
        /// <returns cref="AccountTypeResponse"></returns>
        [OperationContract]
        Task<AccountTypeResponse> AddOrUpdateAccountTypeAsync(AddOrUpdateAccountTypeRequest request);

        /// <summary>
        /// Account Service request to create or modify one or many Account Type Classes
        /// </summary>
        /// <param cref="AddOrUpdateAccountTypeClassRequest" name="request"></param>
        /// <returns cref="AccountTypeClassResponse"></returns>
        [OperationContract]
        Task<AccountTypeClassResponse> AddOrUpdateAccountTypeClassAsync(AddOrUpdateAccountTypeClassRequest request);

        /// <summary>
        /// Account Service request for retrieving Accounts
        /// </summary>
        /// <param cref="GetAccountRequest" name="request"></param>
        /// <returns cref="AccountResponse"></returns>
        [OperationContract]
        Task<AccountResponse> GetAccountAsync(GetAccountRequest request);

        /// <summary>
        /// Account Service request for retrieving Limited Account Information
        /// </summary>
        /// <param cref="GetLimitedAccountInfoRequest" name="request"></param>
        /// <returns cref="LimitedAccountInfoResponse"></returns>
        [OperationContract]
        [RequiresPermissions]
        Task<LimitedAccountInfoResponse> GetLimitedAccountInfoAsync(GetLimitedAccountInfoRequest request);

        /// <summary>
        /// Account Service request for retrieving Account Types
        /// </summary>
        /// <param cref="GetAccountTypeRequest" name="request"></param>
        /// <returns cref="AccountTypeResponse"></returns>
        [OperationContract]
        Task<AccountTypeResponse> GetAccountTypeAsync(GetAccountTypeRequest request);

        /// <summary>
        /// Account Service request for retrieving Account Type Classes
        /// </summary>
        /// <param cref="GetAccountTypeClassRequest" name="request"></param>
        /// <returns cref="AccountTypeClassResponse"></returns>
        [OperationContract]
        Task<AccountTypeClassResponse> GetAccountTypeClassAsync(GetAccountTypeClassRequest request);

        /// <summary>
        /// Account Service request for retrieving Account routing number information
        /// </summary>
        /// <param cref="GetRoutingNumberInfoRequest" name="request"></param>
        /// <returns cref="GetRoutingNumberInfoResponse"></returns>
        [OperationContract]
        Task<GetRoutingNumberInfoResponse> GetRoutingNumberInfoAsync(GetRoutingNumberInfoRequest request);

        /// <summary>
        /// Endpoint for retrieving a list of valid fields that may be used by a given Account Type.
        /// Please note: This endpoint does not return configured fields for existing Account Types. 
        /// To retrieve configured Account Type Fields associated to a given Account Type, you need to call the GetAccountTypeAsync endpoint with IncludeAccountTypeFields Mapper value enabled.
        /// Please see GetAccountTypeFieldsAsync handler/implementation for more detailed explanation.
        /// </summary>
        /// <param cref="GetAccountTypeFieldsRequest" name="request"></param>
        /// <returns cref="GetAccountTypeFieldsResponse"></returns>
        [OperationContract]
        Task<GetAccountTypeFieldsResponse> GetAccountTypeFieldsAsync(GetAccountTypeFieldsRequest request);

        /// <summary>
        /// Account Service request for creating or updating a list of fields for Account Type display
        /// </summary>
        /// <param cref="AddOrUpdateAccountTypeFieldsRequest" name="request"></param>
        /// <returns cref="AccountTypeFieldsResponse"></returns>
        [OperationContract]
        Task<AccountTypeFieldsResponse> AddOrUpdateAccountTypeFieldsAsync(AddOrUpdateAccountTypeFieldsRequest request);

        /// <summary>
        /// Account Service request for deleting an unused account type class
        /// </summary>
        /// <param cref="DeleteAccountTypeClassRequest" name="request"></param>
        /// <returns cref="DeleteAccountTypeClassResponse"></returns>
        [OperationContract]
        Task<DeleteAccountTypeClassResponse> DeleteAccountTypeClassAsync(DeleteAccountTypeClassRequest request);

        /// <summary>
        /// Account Service request for deleting an unused account type
        /// </summary>
        /// <param cref="DeleteAccountTypeRequest" name="request"></param>
        /// <returns cref="DeleteAccountTypeResponse"></returns>
        [OperationContract]
        Task<DeleteAccountTypeResponse> DeleteAccountTypeAsync(DeleteAccountTypeRequest request);

        /// <summary>
        /// Account Service request for deleting a list of account type fields for Account Type display
        /// </summary>
        /// <param cref="DeleteAccountTypeFieldsRequest" name="request"></param>
        /// <returns cref="DeleteAccountTypeFieldsResponse"></returns>
        [OperationContract]
        Task<DeleteAccountTypeFieldsResponse> DeleteAccountTypeFieldsAsync(DeleteAccountTypeFieldsRequest request);

        /// <summary>
        /// Account Service request for updating the LastTransactionSyncFinished property for an account.
        /// </summary>
        /// <param cref="UpdateAccountLastTransactionSyncFinishedRequest" name="request"></param>
        /// <returns cref="UpdateAccountLastTransactionSyncFinishedResponse"></returns>
        [OperationContract]
        Task<UpdateAccountLastTransactionSyncFinishedResponse> UpdateAccountLastTransactionSyncFinishedAsync(UpdateAccountLastTransactionSyncFinishedRequest request);

        /// <summary>
        /// Account Service request for retrieving properly formatted account numbers.
        /// </summary>
        /// <param cref="GetFormattedAccountNumbersRequest" name="request"></param>
        /// <returns cref="GetFormattedAccountNumbersResponse"></returns>
        [OperationContract]
        Task<GetFormattedAccountNumbersResponse> GetFormattedAccountNumbersAsync(GetFormattedAccountNumbersRequest request);

        //
        // The below contract items are obsolete and are pending deletion //
        //

        [OperationContract]
        [Obsolete("Please refer to the Transactions Microservice.", true)]
        Task<TransactionResponse> AddOrUpdateTransactionAsync(AddOrUpdateTransactionRequest request);

        [OperationContract]
        [Obsolete("Please refer to the Transactions Microservice.", true)]
        Task<TransactionCategoryResponse> AddOrUpdateTransactionCategoriesAsync(AddOrUpdateTransactionCategoriesRequest request);

        [OperationContract]
        [Obsolete("Please refer to the Transactions Microservice.", true)]
        Task<TransactionResponse> GetTransactionAsync(GetTransactionRequest request);

        [OperationContract]
        [Obsolete("Please refer to the Transactions Microservice.", true)]
        Task<TransactionCategoryResponse> GetTransactionCategoriesAsync(GetTransactionCategoriesRequest request);
    }
}