using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Alkami.Utilities.Formatting.Expressions
{
    internal class DetokenizerPropertyBinderExpression : PropertyBindingExpressionBase
    {
        internal static readonly Regex RegexDetokenizerRegex = new Regex(@"{([a-zA-Z]\w+\.*\w+), ~(.+?)~}");
        internal static readonly Regex PaddingDetokenizerRegex = new Regex(@"{([a-zA-Z]\w+\.*\w+), \+(.)([rRlL])(\d+)}");
        internal static readonly Regex PropertyPathRegex = new Regex(@"{([a-zA-Z0-9.]+\w+)}");

        private static readonly Regex FormattingRegexDetector = new Regex(@"([a-zA-Z]\w+\.*\w+), ~(.+?)~");
        private static readonly Regex PaddingRegexDetector = new Regex(@"([a-zA-Z]\w+\.*\w+), \+(.)([rRlL])(\d+)");

        internal DetokenizerPropertyBinderExpression(string propertyAccessorExpression, string originalExpression, string format)
            : base(propertyAccessorExpression, originalExpression, format)
        {

        }

        internal override string ProduceTokenFor(object model, PropertyInfo property)
        {
            var propertyAccessorExpressionValue = GetPropertyAccessorExpressionValue(PropertyName, model, property);

            if (propertyAccessorExpressionValue == null)
                return string.Empty;

            var resultToken = propertyAccessorExpressionValue.ToString();

            if (FormattingRegexDetector.IsMatch(OriginalExpression))
            {
                var matchData = FormattingRegexDetector.Match(OriginalExpression);
                var formatRegex = new Regex(matchData.Groups[2].Value.Replace("~", string.Empty));
                var formatRegexMatches = formatRegex.Match(resultToken);
                var builder = new StringBuilder();

                foreach (var group in formatRegexMatches.Groups.OfType<Group>())
                {
                    if (!group.Value.Equals(resultToken, StringComparison.OrdinalIgnoreCase))
                        builder.Append(group.Value);
                }

                return builder.ToString();
            }

            if (PaddingRegexDetector.IsMatch(OriginalExpression))
            {
                var matchData = PaddingRegexDetector.Match(OriginalExpression);
                var paddingCharacter = matchData.Groups[2].Value;
                var isLeftPaddingOperation = string.Compare(matchData.Groups[3].Value, "L", ignoreCase: true) == 0;
                var padLength = 0;

                if (int.TryParse(matchData.Groups[4].Value, out padLength))
                {
                    if (isLeftPaddingOperation)
                        return resultToken.PadLeft(padLength, paddingCharacter.First());

                    return resultToken.PadRight(padLength, paddingCharacter.First());
                }
            }

            return resultToken;
        }

        internal override PropertyInfo LocateMatchingProperty(IEnumerable<PropertyInfo> eligibleProperties)
        {
            var rootPropertyName = PropertyName.Split('.').First();

            return eligibleProperties.FirstOrDefault(property => property.Name == rootPropertyName);
        }

        internal object GetPropertyAccessorExpressionValue(string propertyAccessorExpression, object model, PropertyInfo property)
        {
            if (string.IsNullOrWhiteSpace(propertyAccessorExpression))
                throw new InvalidOperationException($"The property accessor expression was invalid for expression [{OriginalExpression}]");

            var expressionValue = property.GetValue(model);

            if (expressionValue == null)
                return null;

            var remainingAccessorComponents = propertyAccessorExpression.Split('.').Skip(1);

            foreach (var component in remainingAccessorComponents)
            {
                var matchingProperty = expressionValue.GetType().GetProperties().FirstOrDefault(detectedProperty => detectedProperty.Name == component);

                if (matchingProperty == null)
                    return null;

                expressionValue = matchingProperty.GetValue(expressionValue);
            }

            return expressionValue;
        }
    }
}
