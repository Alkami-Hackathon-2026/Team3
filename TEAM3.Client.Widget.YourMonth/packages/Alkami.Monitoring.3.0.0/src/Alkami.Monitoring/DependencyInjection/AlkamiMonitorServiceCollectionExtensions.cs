#if NET6_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Alkami.Monitoring;
using Alkami.Monitoring.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alkami.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions for Alkami Monitoring.
    /// </summary>
    public static class AlkamiMonitorServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <remarks>
        /// The <typeparamref name="TImplementation"/> is used for sending data to for monitoring the application
        /// </remarks>
        /// <typeparam name="TImplementation">The implementation of <see cref="IMonitorMinion"/> to register</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add services.</param>
        /// <returns>A reference to the provided <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddAlkamiMonitoring<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
            where TImplementation : class, IMonitorMinion
        {
            TryAddAlkamiMonitoring(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMonitorMinion), typeof(TImplementation)));


            return services;
        }

        /// <summary>
        /// Registers the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <remarks>
        /// The <typeparamref name="TImplementation"/> is used for sending data to for monitoring the application
        /// </remarks>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationFactory">The factory to lookup the implmentation of <see cref="IMonitorMinion"/></param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAlkamiMonitoring<TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TImplementation : class, IMonitorMinion
        {
            TryAddAlkamiMonitoring(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMonitorMinion), implementationFactory));

            return services;
        }

        /// <summary>
        /// Registers the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <remarks>
        /// The <typeparamref name="TImplementation"/> is used for sending data to for monitoring the application
        /// </remarks>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationInstance">The specific instance.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAlkamiMonitoring<TImplementation>(this IServiceCollection services, TImplementation implementationInstance)
            where TImplementation : class, IMonitorMinion
        {
            TryAddAlkamiMonitoring(services);

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMonitorMinion), implementationInstance));

            return services;
        }

        private static void TryAddAlkamiMonitoring(IServiceCollection services)
        {
            if (services.Any(x => x.ImplementationType == typeof(CompositeMonitor)))
            {
                return;
            }

            var monitorInstance = Metric.MonitorFactory == null ? null : Metric.MonitorFactory();

            if (monitorInstance == null)
            {
                services.AddHostedService<SetupMonitoringHost>();
                services.TryAddSingleton<CompositeMonitor>();
            }
            else
            {
                services.TryAddSingleton(sp =>
                    monitorInstance as CompositeMonitor ?? ActivatorUtilities.CreateInstance<CompositeMonitor>(sp));
            }

            services.TryAddSingleton<IMonitorSuppressionProvider, MonitorSuppressionProvider>();
            services.TryAddSingleton<IMonitor>(x => x.GetRequiredService<CompositeMonitor>());
            services.TryAddSingleton<IMonitorProperties>(x => x.GetRequiredService<CompositeMonitor>());

        }
    }
}
#endif
