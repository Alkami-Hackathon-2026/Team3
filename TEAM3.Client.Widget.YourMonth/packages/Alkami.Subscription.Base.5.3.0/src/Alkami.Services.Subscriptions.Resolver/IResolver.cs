using Alkami.Services.Subscriptions.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.Resolver
{
    public interface IResolver : IParticipatingService
    {
        string ClientVersion { get; set; }

        event Action<List<ServiceDefinition>> OnChange;
        void Disconnecting(ServiceDefinition serviceDefinition);

        void Disconnecting(List<ServiceDefinition> serviceDefinitions);

        Task DisconnectingAsync(ServiceDefinition serviceDefinition);

        Task DisconnectingAsync(List<ServiceDefinition> serviceDefinitions);

        Task RefreshData(bool forceRefresh = false);
        List<ServiceDefinition> AllRegisteredDefinitions { get; }
    }
}
