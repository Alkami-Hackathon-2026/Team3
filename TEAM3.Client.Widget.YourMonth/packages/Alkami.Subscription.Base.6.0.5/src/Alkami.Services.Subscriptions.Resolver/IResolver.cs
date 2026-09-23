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

        [Obsolete("Please use DisconnectingAsync")]
        void Disconnecting(ServiceDefinition serviceDefinition);

        [Obsolete("Please use DisconnectingAsync")]
        void Disconnecting(List<ServiceDefinition> serviceDefinitions);

        Task DisconnectingAsync(ServiceDefinition serviceDefinition);

        Task DisconnectingAsync(List<ServiceDefinition> serviceDefinitions);

        Task RefreshData(bool forceRefresh = false);

        List<ServiceDefinition> AllRegisteredDefinitions { get; }
    }
}
