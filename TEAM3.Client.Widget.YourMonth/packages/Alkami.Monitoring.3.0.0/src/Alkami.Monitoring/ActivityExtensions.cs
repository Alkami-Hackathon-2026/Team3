#if NET6_0_OR_GREATER
using System.Diagnostics;

namespace Alkami.Monitoring
{
    /// <summary>
    /// <see cref="Activity"/> extension methods.
    /// </summary>
    internal static class ActivityExtensions
    {
        /// <summary>
        /// Gets the span unique identifier regardless of the activity identifier format.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The span unique identifier.</returns>
        public static string GetSpanId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.W3C => activity.SpanId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }

        /// <summary>
        /// Gets the span trace unique identifier regardless of the activity identifier format.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The span trace unique identifier.</returns>
        public static string GetTraceId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.W3C => activity.TraceId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }

        /// <summary>
        /// Gets the span parent unique identifier regardless of the activity identifier format.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The span parent unique identifier.</returns>
        public static string GetParentId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.W3C => activity.ParentSpanId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }
    }
}
#endif
