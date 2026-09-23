using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.Resolver;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public class ServiceEndpointRepository<T> : IServiceEndpointRepository<T> where T : class
    {
        private ILog _logger;
        private List<ServiceEndpoint<T>> _serviceEndpoints;
        private object _objLock;
        private bool _isInitialized;

        private ServiceEndpointRepository()
        {

        }
        public ServiceEndpointRepository(ILog logger)
        {
            _logger = logger;
            _serviceEndpoints = new List<ServiceEndpoint<T>>();
            _objLock = new object();
        }

        public IEnumerable<ServiceEndpoint<T>> GetServiceEndpoints()
        {
            return _serviceEndpoints;
        }

        public void InitializeServiceResolverListener()
        {
            if (!_isInitialized)
            {
                lock (_objLock)
                {
                    if (!_isInitialized) //checking again in case of a second thread that is unblocked and attempts this again
                    {
                        ServiceResolver.OnChange += UpdateServiceEndpoints;
                        UpdateServiceEndpoints(ServiceResolver.AllRegisteredDefinitions());

                        _isInitialized = true;
                    }
                }
            }
        }

        public void UpdateServiceEndpoints(List<ServiceDefinition> serviceDefinitions)
        {
            try
            {
                var typeName = typeof(T).FullName;
                List<ServiceDefinition> latestDefinitions = null;

                if (typeName == "Alkami.Subscriptions.ProviderBased.Contracts.IPluginContract")
                {
                    latestDefinitions = serviceDefinitions.Where(x => x.ProviderConfiguration != null).ToList();
                }
                else
                {
                    latestDefinitions = serviceDefinitions.Where(x => x.Name == typeName).ToList();
                }

                if (_logger.IsTraceEnabled)
                {
                    _logger.TraceFormat("Found {0} definitions", latestDefinitions.Count);
                }

                latestDefinitions = latestDefinitions.GroupBy(d => d.EndpointUri).Select(g => g.First()).ToList();

                lock (_objLock)
                {
                    var currentUriEndPoints = _serviceEndpoints.Select(x => x.Endpoint).ToList();
                    var latestServiceEndPoints = new List<ServiceEndpoint<T>>();

                    if (_logger.IsTraceEnabled)
                    {
                        foreach (var definition in latestDefinitions)
                        {
                            if (currentUriEndPoints.All(x => x != definition.EndpointUri))
                            {
                                _logger.TraceFormat("New definition found at: {0}, with version: {1}", definition.EndpointUri, definition.CurrentVersion);
                            }
                        }
                    }

                    foreach (var definition in latestDefinitions)
                    {
                        var se = new ServiceEndpoint<T>
                        {
                            Endpoint = definition.EndpointUri,
                            ResolvesFor = typeName,
                            ServiceDefinition = definition,
                            ProviderConfiguration = definition.ProviderConfiguration,
                        };

                        latestServiceEndPoints.Add(se);
                    }

                    _serviceEndpoints.Clear();
                    _serviceEndpoints.AddRange(latestServiceEndPoints);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred when processing endpoints for type {typeof(T).FullName}", e);
            }
        }
        public void ServiceEndpointHostHasIssues(Uri uri)
        {
            var serviceEndpointsToShutdown = _serviceEndpoints
                .Where(x => x.Endpoint == uri)
                .ToArray();

            ShutdownAndRemoveServiceEndPoint(serviceEndpointsToShutdown);
        }
        public void ShutdownAndRemoveServiceEndPoint(params ServiceEndpoint<T>[] serviceEndpoints)
        {
            lock (_objLock)
            {
                foreach (var serviceEndPoint in serviceEndpoints)
                {
                    serviceEndPoint.ShutDown();
                    _serviceEndpoints.Remove(serviceEndPoint);
                }
            }
        }
    }
}
