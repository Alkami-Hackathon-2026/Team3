using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Alkami.Monitoring.Implementation
{
    internal static class MonitorMinionAliasUtilities
    {
        private const string AliasAttributeTypeFullName = "Alkami.Monitoring.MonitorMinionAliasAttribute";

        internal static string GetAlias(Type providerType)
        {
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(providerType);

            for (int i = 0; i < attributes.Count; i++)
            {
                CustomAttributeData attributeData = attributes[i];
                if (attributeData.AttributeType.FullName == AliasAttributeTypeFullName &&
                    attributeData.ConstructorArguments.Count > 0)
                {
                    CustomAttributeTypedArgument arg = attributeData.ConstructorArguments[0];

                    Debug.Assert(arg.ArgumentType == typeof(string));

                    return arg.Value?.ToString();
                }
            }

            return providerType.Name;
        }
    }
}
