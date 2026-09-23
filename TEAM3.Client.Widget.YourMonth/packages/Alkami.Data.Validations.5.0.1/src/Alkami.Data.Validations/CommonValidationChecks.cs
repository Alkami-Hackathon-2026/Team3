using System;
using System.Collections.Generic;

namespace Alkami.Data.Validations
{
    /// <summary>
    /// The <see cref="CommonValidationChecks"/> class contains various common validation checks.
    /// </summary>
    public static class CommonValidationChecks
    {
        /// <summary>
        /// Checks to see if the provided value is null, empty, or whitespace and, if so, adds a <see cref="ValidationResult"/> to the provided list.
        /// </summary>
        /// <param name="results">The list to add <see cref="ValidationResult"/>s.</param>
        /// <param name="value">The value of the field to check.</param>
        /// <param name="fieldName">The name of the field being checked.</param>
        public static void AddErrorIfStringIsNullEmptyOrWhitespace(this List<ValidationResult> results, string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                results.AddValidationError
                (
                    fieldName,
                    string.Format("{0} cannot be null, empty, or whitespace.", fieldName),
                    SubCode.ValueUnsupported
                );
            }
        }

        /// <summary>
        /// Checks to see if the provided value is not defined in the Enum and, if so, adds a <see cref="ValidationResult"/> to the provided list.
        /// </summary>
        /// <typeparam name="T">The type of the enum to check.</typeparam>
        /// <param name="results">The list to add <see cref="ValidationResult"/>s.</param>
        /// <param name="value">The value of the field to check.</param>
        /// <param name="fieldName">The name of the field being checked.</param>
        public static void AddErrorIfEnumValueIsNotDefined<T>(this List<ValidationResult> results, T value, string fieldName) where T : struct, IConvertible
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                results.AddValidationError
                (
                    fieldName,
                    string.Format("{0} is not a valid value.", fieldName),
                    SubCode.ValueUnsupported
                );
            }
        }

        /// <summary>
        /// Checks to see if the provided value is null and, if so, adds a <see cref="ValidationResult"/> to the provided list.
        /// </summary>
        /// <typeparam name="T">The type of the value to check.</typeparam>
        /// <param name="results">The list to add <see cref="ValidationResult"/>s.</param>
        /// <param name="value">The value of the field to check.</param>
        /// <param name="fieldName">The name of the field being checked.</param>
        public static void AddErrorIfNull<T>(this List<ValidationResult> results, T value, string fieldName) where T : class
        {
            if (value == null)
            {
                results.AddValidationError
                (
                    fieldName,
                    string.Format("{0} cannot be null.", fieldName),
                    SubCode.ValueUnsupported
                );
            }
        }

        /// <summary>
        /// Checks to see if the provided value is Guid.Empty and, if so, adds a <see cref="ValidationResult"/> to the provided list.
        /// </summary>
        /// <param name="results">The list to add <see cref="ValidationResult"/>s.</param>
        /// <param name="value">The value of the field to check.</param>
        /// <param name="fieldName">The name of the field being checked.</param>
        public static void AddErrorIfGuidEmpty(this List<ValidationResult> results, Guid value, string fieldName)
        {
            if (value == Guid.Empty)
            {
                results.AddValidationError
                (
                    fieldName,
                    string.Format("{0} cannot be empty.", fieldName),
                    SubCode.ValueUnsupported
                );
            }
        }

        /// <summary>
        /// Performs a comparison based on the provided values and the desired <see cref="Comparison"/> and, if true, adds a <see cref="ValidationResult"/> to the provided list.
        /// </summary>
        /// <typeparam name="T">The type of the value to compare.</typeparam>
        /// <param name="results">The list to add <see cref="ValidationResult"/>s.</param>
        /// <param name="actualValue">The actual value to test.</param>
        /// <param name="comparison">The <see cref="Comparison"/> type to perform.</param>
        /// <param name="expectedValue">The expected (constant) value to compare against.</param>
        /// <param name="fieldName">The name of the field being checked.</param>
        public static void AddErrorIf<T>(this List<ValidationResult> results, T actualValue, Comparison comparison, T expectedValue, string fieldName) where T : IComparable<T>
        {
            var comparisonResult = actualValue.CompareTo(expectedValue);
            string messageFormat;

            if ((comparison == Comparison.Equal) && (comparisonResult == 0))
                messageFormat = "{0} cannot be equal to {1}.";
            else if ((comparison == Comparison.GreaterThan) && (comparisonResult > 0))
                messageFormat = "{0} cannot be greater than {1}.";
            else if ((comparison == Comparison.GreaterThanOrEqual) && (comparisonResult >= 0))
                messageFormat = "{0} cannot be greater than or equal to {1}.";
            else if ((comparison == Comparison.LessThan) && (comparisonResult < 0))
                messageFormat = "{0} cannot be less than {1}.";
            else if ((comparison == Comparison.LessThanOrEqual) && (comparisonResult <= 0))
                messageFormat = "{0} cannot be less than or equal to {1}.";
            else if ((comparison == Comparison.NotEqual) && (comparisonResult != 0))
                messageFormat = "{0} must be equal to {1}.";
            else
                messageFormat = null;

            if (messageFormat != null)
            {
                results.AddValidationError
                (
                    fieldName,
                    string.Format(messageFormat, fieldName, expectedValue),
                    SubCode.ValueOutOfRange
                );
            }
        }

        /// <summary>
        /// Adds a validation error to the provided collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <param name="results">The collection of <see cref="ValidationResult"/>s to add to.</param>
        /// <param name="fieldName">The name of the field with the validation error.</param>
        /// <param name="message">The message to add to the <see cref="ValidationResult"/>.</param>
        /// <param name="subCode">The <see cref="SubCode"/> for the validation.</param>
        public static void AddValidationError(this List<ValidationResult> results, string fieldName, string message = "", SubCode subCode = SubCode.None)
        {
            results.Add(new ValidationResult()
            {
                ErrorCode = ErrorCode.ValidationError,
                SubCode = subCode,
                Field = fieldName,
                Message = message,
                Severity = Severity.Error,
            });
        }

        /// <summary>
        /// Adds a validation error to the provided collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <param name="results">The collection of <see cref="ValidationResult"/>s to add to.</param>
        /// <param name="fieldName">The name of the field with the validation error.</param>
        /// <param name="message">The message to add to the <see cref="ValidationResult"/>.</param>
        /// <param name="customSubCode">The custom sub code for the validation.</param>
        public static void AddCustomValidationError(this List<ValidationResult> results, string fieldName, string message = "", string customSubCode = "")
        {
            results.Add(new ValidationResult()
            {
                ErrorCode = ErrorCode.ValidationError,
                SubCode = SubCode.Custom,
                Field = fieldName,
                Message = message,
                CustomSubCode = customSubCode,
                Severity = Severity.Error
            });
        }

        /// <summary>
        /// Adds a validation warning to the provided collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <param name="results">The collection of <see cref="ValidationResult"/>s to add to.</param>
        /// <param name="fieldName">The name of the field with the validation warning.</param>
        /// <param name="message">The message to add to the <see cref="ValidationResult"/>.</param>
        /// <param name="subCode">The <see cref="SubCode"/> for the validation.</param>
        public static void AddValidationWarning(this List<ValidationResult> results, string fieldName, string message = "", SubCode subCode = SubCode.None)
        {
            results.Add(new ValidationResult()
            {
                ErrorCode = ErrorCode.ValidationError,
                SubCode = subCode,
                Field = fieldName,
                Message = message,
                Severity = Severity.Warning,
            });
        }

        /// <summary>
        /// Adds a validation warning to the provided collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <param name="results">The collection of <see cref="ValidationResult"/>s to add to.</param>
        /// <param name="fieldName">The name of the field with the validation warning.</param>
        /// <param name="message">The message to add to the <see cref="ValidationResult"/>.</param>
        /// <param name="customSubCode">The custom sub code for the validation.</param>
        public static void AddCustomValidationWarning(this List<ValidationResult> results, string fieldName, string message = "", string customSubCode = "")
        {
            results.Add(new ValidationResult()
            {
                ErrorCode = ErrorCode.ValidationError,
                SubCode = SubCode.Custom,
                Field = fieldName,
                Message = message,
                Severity = Severity.Warning,
                CustomSubCode = customSubCode
            });
        }
    }
}