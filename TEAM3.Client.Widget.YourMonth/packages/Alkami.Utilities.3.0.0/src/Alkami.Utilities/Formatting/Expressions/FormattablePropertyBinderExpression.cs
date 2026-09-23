using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Alkami.Utilities.Formatting.Expressions
{
    internal class FormattablePropertyBinderExpression : PropertyBindingExpressionBase
    {
        internal static readonly Regex DetectorRegex = new Regex(@"{([a-zA-Z]\w+\.*\w+):([^}]+)}");

        internal FormattablePropertyBinderExpression(string propertyName, string originalExpression, string format)
            : base(propertyName, originalExpression, format)
        {

        }

        internal override string ProduceTokenFor(object model, PropertyInfo property)
        {
            var definedValue = property.GetValue(model);

            if (definedValue != null)
            {
                IFormattable formattable = null;

                if (!string.IsNullOrWhiteSpace(Format) && ((formattable = (definedValue as IFormattable)) != null))
                {
                    return formattable.ToString(Format, CultureInfo.InvariantCulture);
                }
                else
                {
                    return definedValue.ToString();
                }
            }

            return string.Empty;
        }
    }
}
