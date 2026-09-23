using Alkami.Data.Validations;
using Alkami.MicroServices.Accounts.Contracts;
using Alkami.MicroServices.Accounts.Contracts.Requests;
using Alkami.MicroServices.Accounts.Contracts.Responses;
using Alkami.Services.Subscriptions.ParticipatingClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Accounts.Service.Client
{
    /// <summary>
    /// Account Service Client
    /// </summary>
    public class AccountServiceClient : SelfResolvingClient<IAccountServiceContract>, IAccountServiceContract
	{
        /// <inheritdoc/>
		public Task<AccountResponse> AddOrUpdateAccountAsync(AddOrUpdateAccountRequest request)
		{
			return this.ProxyCall((c, r) => c.AddOrUpdateAccountAsync(r), request);
		}

        /// <inheritdoc/>
		public Task<AccountTypeResponse> AddOrUpdateAccountTypeAsync(AddOrUpdateAccountTypeRequest request)
		{
			return this.ProxyCall((c, r) => c.AddOrUpdateAccountTypeAsync(r), request);
		}

        /// <inheritdoc/>
		public Task<AccountTypeClassResponse> AddOrUpdateAccountTypeClassAsync(AddOrUpdateAccountTypeClassRequest request)
		{
			return this.ProxyCall((c, r) => c.AddOrUpdateAccountTypeClassAsync(r), request);
		}

        /// <inheritdoc/>
        public Task<AccountResponse> GetAccountAsync(GetAccountRequest request)
		{
			return this.ProxyCall((c, r) => c.GetAccountAsync(r), request);
		}

        /// <inheritdoc/>
		public Task<LimitedAccountInfoResponse> GetLimitedAccountInfoAsync(GetLimitedAccountInfoRequest request)
		{
			return this.ProxyCall((c, r) => c.GetLimitedAccountInfoAsync(r), request);
		}

        /// <inheritdoc/>
		public Task<AccountTypeResponse> GetAccountTypeAsync(GetAccountTypeRequest request)
		{
			return this.ProxyCall((c, r) => c.GetAccountTypeAsync(r), request);
		}

        /// <inheritdoc/>
		public Task<AccountTypeClassResponse> GetAccountTypeClassAsync(GetAccountTypeClassRequest request)
		{
			return this.ProxyCall((c, r) => c.GetAccountTypeClassAsync(r), request);
		}

        /// <inheritdoc/>
        public Task<GetRoutingNumberInfoResponse> GetRoutingNumberInfoAsync(GetRoutingNumberInfoRequest request)
        {
            return ProxyCall((c, r) => c.GetRoutingNumberInfoAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<GetAccountTypeFieldsResponse> GetAccountTypeFieldsAsync(GetAccountTypeFieldsRequest request)
        {
            return ProxyCall((c, r) => c.GetAccountTypeFieldsAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<AccountTypeFieldsResponse> AddOrUpdateAccountTypeFieldsAsync(AddOrUpdateAccountTypeFieldsRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateAccountTypeFieldsAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteAccountTypeClassResponse> DeleteAccountTypeClassAsync(DeleteAccountTypeClassRequest request)
        {
            return ProxyCall((c, r) => c.DeleteAccountTypeClassAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteAccountTypeResponse> DeleteAccountTypeAsync(DeleteAccountTypeRequest request)
        {
            return ProxyCall((c, r) => c.DeleteAccountTypeAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteAccountTypeFieldsResponse> DeleteAccountTypeFieldsAsync(DeleteAccountTypeFieldsRequest request)
        {
            return ProxyCall((c, r) => c.DeleteAccountTypeFieldsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<GetFormattedAccountNumbersResponse> GetFormattedAccountNumbersAsync(
            GetFormattedAccountNumbersRequest request)
        {
            return ProxyCall((c, r) => c.GetFormattedAccountNumbersAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<UpdateAccountLastTransactionSyncFinishedResponse> UpdateAccountLastTransactionSyncFinishedAsync(UpdateAccountLastTransactionSyncFinishedRequest request)
        {
            return ProxyCall((c, r) => c.UpdateAccountLastTransactionSyncFinishedAsync(r), request);
        }

        #region OBSOLETE
        /// <inheritdoc/>
		public Task<TransactionResponse> AddOrUpdateTransactionAsync(AddOrUpdateTransactionRequest request)
        {
            //return this.ProxyCall((c, r) => c.AddOrUpdateTransactionAsync(r), request);
            return Task.FromResult<TransactionResponse>(
                new TransactionResponse
                {
                    HasError = true,
                    ValidationResults = new List<ValidationResult>
                    {
                        new ValidationResult
                        {
                            ErrorCode = ErrorCode.UnsupportedError,
                            Message = "This endpoint is no longer supported; call the Transactions Microservice."
                        }
                    }
                }
            );
        }
        /// <inheritdoc/>
		public Task<TransactionCategoryResponse> AddOrUpdateTransactionCategoriesAsync(AddOrUpdateTransactionCategoriesRequest request)
        {
            //return this.ProxyCall((c, r) => c.AddOrUpdateTransactionCategoriesAsync(r), request);
            return Task.FromResult<TransactionCategoryResponse>(
                new TransactionCategoryResponse
                {
                    HasError = true,
                    ValidationResults = new List<ValidationResult>
                    {
                        new ValidationResult
                        {
                            ErrorCode = ErrorCode.UnsupportedError,
                            Message = "This endpoint is no longer supported; call the Transactions Microservice."
                        }
                    }
                }
            );
        }
        /// <inheritdoc/>
        public Task<TransactionResponse> GetTransactionAsync(GetTransactionRequest request)
		{
            //return this.ProxyCall((c, r) => c.GetTransactionAsync(r), request);
            return Task.FromResult<TransactionResponse>(
                new TransactionResponse
                {
                    HasError = true,
                    ValidationResults = new List<ValidationResult>
                    {
                        new ValidationResult
                        {
                            ErrorCode = ErrorCode.UnsupportedError,
                            Message = "This endpoint is no longer supported; call the Transactions Microservice."
                        }
                    }
                }
            );
        }
        /// <inheritdoc/>
        public Task<TransactionCategoryResponse> GetTransactionCategoriesAsync(GetTransactionCategoriesRequest request)
		{
            //return this.ProxyCall((c, r) => c.GetTransactionCategoriesAsync(r), request);
            return Task.FromResult<TransactionCategoryResponse>(
                new TransactionCategoryResponse
                {
                    HasError = true,
                    ValidationResults = new List<ValidationResult>
                    {
                        new ValidationResult
                        {
                            ErrorCode = ErrorCode.UnsupportedError,
                            Message = "This endpoint is no longer supported; call the Transactions Microservice."
                        }
                    }
                }
            );
        }
        #endregion
    }
}