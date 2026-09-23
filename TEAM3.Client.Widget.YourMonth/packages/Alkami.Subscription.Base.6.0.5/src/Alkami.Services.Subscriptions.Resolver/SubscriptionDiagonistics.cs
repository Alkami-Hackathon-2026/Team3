using System.Diagnostics;
using System.Reflection;

namespace Alkami.Services.Subscriptions.Resolver
{
    internal class SubscriptionDiagonistics
    {
        internal static readonly Assembly Assembly = typeof(SubscriptionDiagonistics).Assembly;
        internal static readonly AssemblyName AssemblyName = Assembly.GetName();
        internal static readonly string ActivitySourceName = AssemblyName.Name!;
        internal static readonly string MeterName = AssemblyName.Name!;

        public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
    }
}
