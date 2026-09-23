using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Accounts.Contracts
{
    internal static class ObjectPrinterExtensions
    {
        internal static string Print<T>(this IEnumerable<T> sourceEnumerable)
        {
            if (sourceEnumerable == null || !sourceEnumerable.Any())
                return "(empty)";

            return string.Join(",", sourceEnumerable);
        }

        internal static string Print<T>(this Nullable<T> nullable)
            where T : struct
        {
            if (!nullable.HasValue)
                return "(not set)";

            return nullable.Value.ToString();
        }
    }
}
