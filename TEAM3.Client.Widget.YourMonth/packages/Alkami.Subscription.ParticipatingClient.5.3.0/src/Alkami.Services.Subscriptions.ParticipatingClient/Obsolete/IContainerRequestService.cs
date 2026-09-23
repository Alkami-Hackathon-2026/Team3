using Alkami.Contracts;
using System;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public interface IContainerRequestService<T> where T : class
    {
        /// <summary>
        /// Makes the request to the container via the <paramref name="serviceUrl"/>
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="operation"></param>
        /// <param name="request"></param>
        /// <param name="serviceUrl"></param>
        /// <returns></returns>
        [Obsolete]
        Task<TResponse> MakeRequestAsync<TRequest, TResponse>(Func<T, TRequest, Task<TResponse>> operation, TRequest request, Uri serviceUrl)
            where TRequest : BaseRequest
            where TResponse : BaseResponse, new();

        /// <summary>
        /// Applies the SUT to the <paramref name="request"/> based on <see cref="BaseRequest.ClaimsIdentity"/> or the Current ThreadPrincipal
        /// </summary>
        /// <param name="request"></param>
        /// <param name="favoredSerializationMethod"></param>
        /// <param name="allowedSerializationMethods"></param>
        /// <returns></returns>
        [Obsolete]
        Task ApplySutToRequestAsync(BaseRequest request, ClaimsIdentitySerializationMethod favoredSerializationMethod, ClaimsIdentitySerializationMethod allowedSerializationMethods);
    }
}
