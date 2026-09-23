using System;

namespace Alkami.Utilities.Certificates.Extensions
{
    internal static class UriExtensions
    {
        internal static Uri Append(this Uri uri, string? appendPath)
        {
            var combined = $"{uri.AbsoluteUri.TrimEnd('/')}/{appendPath?.TrimStart('/')}";
            return new Uri(combined);
        }
    }
}
