using System;
using System.Collections.Generic;
using Alkami.Contracts;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public interface IServiceEndpointResolver<T> where T : class
    {
        ServiceEndpoint<T> SelectEndPointByMinimumSupportedVersion(BaseRequest request, IEnumerable<ServiceEndpoint<T>> serviceEndpoints, Version minimumSupportedVersion);
        IEnumerable<ServiceEndpoint<T>> SelectEndPointsByMinimumSupportedVersion(IEnumerable<ServiceEndpoint<T>> serviceEndpoints, Version minimumSupportedVersion);
    }
}