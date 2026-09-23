using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alkami.Utilities.Formatting.Expressions
{
    internal abstract class PropertyBindingExpressionBase
    {
        internal string PropertyName { get; set; }
        internal string OriginalExpression { get; set; }
        internal string Format { get; set; }

        internal PropertyBindingExpressionBase(string propertyName, string originalExpression, string format)
        {
            PropertyName = propertyName;
            OriginalExpression = originalExpression;
            Format = format;
        }

        internal abstract string ProduceTokenFor(object model, PropertyInfo property);

        internal virtual PropertyInfo LocateMatchingProperty(IEnumerable<PropertyInfo> eligibleProperties)
        {
            return eligibleProperties.FirstOrDefault(property => property.Name == PropertyName);
        }
    }
}
