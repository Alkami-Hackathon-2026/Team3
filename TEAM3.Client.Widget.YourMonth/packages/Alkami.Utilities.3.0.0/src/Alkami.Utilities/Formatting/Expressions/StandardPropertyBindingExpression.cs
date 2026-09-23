using System.Reflection;
using System.Text.RegularExpressions;

namespace Alkami.Utilities.Formatting.Expressions
{
    internal class StandardPropertyBinderExpression : PropertyBindingExpressionBase
    {
        public static readonly Regex DetectorRegex = new Regex(@"{([a-zA-Z0-9]+\w+)}");

        internal StandardPropertyBinderExpression(string propertyName)
            : base(propertyName, propertyName, string.Empty)
        {

        }

        internal override string ProduceTokenFor(object model, PropertyInfo property)
        {
            var definedValue = property.GetValue(model);

            if (definedValue != null)
                return definedValue.ToString();

            return string.Empty;
        }
    }
}
