using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Alkami.Utilities.Formatting.Expressions;

namespace Alkami.Utilities.Formatting
{
    /// <summary>
    /// This class takes information from the source object and binds it to a format string with qualifiers.
    /// </summary>
    public class ObjectPropertyBinder
    {
        /// <summary>
        /// Gets or sets the PropertyReflector dependency
        /// </summary>
        public PropertyReflector PropertyReflector { get; set; }

        /// <summary>
        /// Creates an instance of the ObjectPropertyBinder class.
        /// </summary>
        public ObjectPropertyBinder()
        {
            PropertyReflector = new PropertyReflector();
        }

        /// <summary>
        /// Binds the specified source data to the output format by the exposed public properties on the source object, with formatting as appropriate.
        /// </summary>
        /// <param name="outputFormat"></param>
        /// <param name="sourceData"></param>
        /// <returns></returns>
        public virtual string Bind(string outputFormat, object sourceData)
        {
            if (outputFormat == null)
                return null;

            if (sourceData == null)
                return outputFormat;

            var eligibleProperties = PropertyReflector.DiscoverEligiblePropertiesFrom(sourceData);
            var tokens = DetectPropertyBindingExpressionsIn(outputFormat);

            foreach (var token in tokens)
            {
                var matchingProperty = token.LocateMatchingProperty(eligibleProperties);

                if (matchingProperty != null)
                    outputFormat = outputFormat.Replace("{" + token.OriginalExpression + "}", token.ProduceTokenFor(sourceData, matchingProperty));
            }

            return outputFormat;
        }

        /// <summary>
        /// This scans the outputFormat for binding expressions, and produces binding expression tokens.
        /// </summary>
        /// <param name="outputFormat"></param>
        /// <returns></returns>
        private IEnumerable<PropertyBindingExpressionBase> DetectPropertyBindingExpressionsIn(string outputFormat)
        {
            var basicReplacementsMatches = StandardPropertyBinderExpression.DetectorRegex.Matches(outputFormat).OfType<Match>();

            foreach (var match in basicReplacementsMatches)
            {
                if (match.Groups != null && match.Groups.Count >= 2)
                    yield return new StandardPropertyBinderExpression(match.Groups[1].Value);
            }

            var formattableReplacementMatches = FormattablePropertyBinderExpression.DetectorRegex.Matches(outputFormat).OfType<Match>();

            foreach (var match in formattableReplacementMatches)
            {
                if (match.Groups != null && match.Groups.Count >= 3)
                {
                    var internalExpression = match.Groups[0].Value.Substring(1, match.Groups[0].Value.Length - 2);
                    var propertyExpression = match.Groups[1].Value;
                    var formatExpression = match.Groups[2].Value;

                    yield return new FormattablePropertyBinderExpression(propertyExpression, internalExpression, formatExpression);
                }
            }

            var paddingMatches = DetokenizerPropertyBinderExpression.PaddingDetokenizerRegex.Matches(outputFormat).OfType<Match>();
            var regexWhitelistMatches = DetokenizerPropertyBinderExpression.RegexDetokenizerRegex.Matches(outputFormat).OfType<Match>();
            var propertyPathReplacementMatches = DetokenizerPropertyBinderExpression.PropertyPathRegex.Matches(outputFormat).OfType<Match>();

            foreach (var match in paddingMatches.Concat(regexWhitelistMatches).Concat(propertyPathReplacementMatches))
            {
                if (match.Groups != null && match.Groups.Count >= 1)
                {
                    var internalExpression = match.Groups[0].Value.Substring(1, match.Groups[0].Value.Length - 2);
                    var components = internalExpression.Split(',');
                    var propertyExpression = components.First();
                    var otherExpressionComponents = string.Join(",", components.Skip(1));

                    yield return new DetokenizerPropertyBinderExpression(propertyExpression, internalExpression, otherExpressionComponents);
                }
            }
        }
    }
}
