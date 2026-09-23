using System;
using System.Collections.Generic;
using Alkami.Services.Subscriptions.Data;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public interface IServiceEndpointRepository<T> where T : class
    {
        IEnumerable<ServiceEndpoint<T>> GetServiceEndpoints();
        void InitializeServiceResolverListener();
        void ServiceEndpointHostHasIssues(Uri uri);
        void ShutdownAndRemoveServiceEndPoint(params ServiceEndpoint<T>[] serviceEndpoints);
        void UpdateServiceEndpoints(List<ServiceDefinition> serviceDefinitions);
    }
}
