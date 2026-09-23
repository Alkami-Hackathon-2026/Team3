using Alkami.Contracts;
using Alkami.Services.Subscriptions.Data;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public class ServiceEndpointResolver<T> : IServiceEndpointResolver<T> where T : class
    {

        public ServiceEndpointResolver(ILog logger)
        {
        }

        public ServiceEndpoint<T> SelectEndPointByMinimumSupportedVersion(BaseRequest request, IEnumerable<ServiceEndpoint<T>> serviceEndpoints,
            Version minimumSupportedVersion)
        {
            var endpoints = SelectEndPointsByMinimumSupportedVersion(serviceEndpoints, minimumSupportedVersion);

            if (endpoints != null)
            {
                var loadBalancer = new RoundRobinLoadBalancer<T>(endpoints.ToList());
                var serviceEndpoint = loadBalancer.GetNext(request);

                return serviceEndpoint;
            }

            return null;
        }

        public IEnumerable<ServiceEndpoint<T>> SelectEndPointsByMinimumSupportedVersion(IEnumerable<ServiceEndpoint<T>> serviceEndpoints,
            Version minimumSupportedVersion)
        {
            if (serviceEndpoints != null && serviceEndpoints.Any())
            {
                var endpointsWithinMajorAndMinVersion = serviceEndpoints.Where(x => x.ServiceDefinition.CurrentVersion.Major == minimumSupportedVersion.Major &&
                    x.ServiceDefinition.CurrentVersion >= minimumSupportedVersion);

                if (endpointsWithinMajorAndMinVersion.Any())
                {
                    return endpointsWithinMajorAndMinVersion;
                }

                var endpointsWithinMajor = serviceEndpoints.Where(x => x.ServiceDefinition.CurrentVersion.Major == minimumSupportedVersion.Major);

                if (endpointsWithinMajor.Any())
                {
                    return endpointsWithinMajor;
                }

                var endpointsAboveMajor = serviceEndpoints.Where(x => x.ServiceDefinition.CurrentVersion.Major > minimumSupportedVersion.Major);

                if (endpointsAboveMajor.Any())
                {
                    return endpointsAboveMajor;
                }

                var maxVersion = serviceEndpoints.Max(x => x.ServiceDefinition.CurrentVersion);

                return serviceEndpoints.Where(x => x.ServiceDefinition.CurrentVersion == maxVersion);
            }

            return new ServiceEndpoint<T>[] { };
        }
    }
}
