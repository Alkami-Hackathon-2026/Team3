using Alkami.Contracts;
using Alkami.Utilities.LegacyIdentity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public class ContainerRequestService
    {

    }

    [Obsolete]
    public class ContainerRequestService<T> : ContainerRequestService, IContainerRequestService<T> where T : class
    {
        private static readonly Lazy<IContainerRequestService<T>> lazy = new Lazy<IContainerRequestService<T>>(() => new ContainerRequestService<T>());

        public static IContainerRequestService<T> Instance { get { return lazy.Value; } }

        [Obsolete]
        public ContainerRequestService() : this(new ClaimsIdentitySerializer(), ParticipatingHttpClientFactory.Instance)
        {

        }

        [Obsolete]
        public ContainerRequestService(IClaimsIdentitySerializer claimsIdentitySerializer, IParticipatingHttpClientFactory httpClientFactory)
        {
        }

        /// <inheritdoc />
        public Task<TResponse> MakeRequestAsync<TRequest, TResponse>(Func<T, TRequest, Task<TResponse>> operation, TRequest request, Uri serviceUrl)
            where TRequest : BaseRequest
            where TResponse : BaseResponse, new()
        {
            return Task.FromResult(default(TResponse));
        }

        /// <inheritdoc />
        public Task ApplySutToRequestAsync(BaseRequest request, ClaimsIdentitySerializationMethod favoredSerializationMethod, ClaimsIdentitySerializationMethod allowedSerializationMethods)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the ClaimIdentity based on either the request or the current CurrentPrincipal
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>To help make compatible with http services we need this to be a Task</remarks>
        /// <returns></returns>
        protected virtual Task<ClaimsIdentity> GetClaimsIdentity(BaseRequest request)
        {
            return Task.FromResult(request.ClaimsIdentity ?? SelfResolvingClient.LegacyShimFactory());
        }
    }
}
