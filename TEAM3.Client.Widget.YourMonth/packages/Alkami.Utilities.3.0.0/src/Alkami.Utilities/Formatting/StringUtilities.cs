using Alkami.Utilities.Extensions;
using Alkami.Utilities.Validation;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Alkami.Utilities.Formatting.Utilities
{
    /// <summary>
    /// Global utilities for string formatting
    /// </summary>
    public static class StringUtilities
    {
        private const int PHONE_NUMBER_LENGTH = 10;

        private static bool ShouldNotSanitize = !GetShouldSanitize();
        private static readonly Regex TaxIdRegex = GetTaxIdRegex();
        private static readonly Regex PrivateNumberRegex = GetPrivateNumberRegex();
        private static readonly Regex ConnectionPasswordRegex = GetConnectionPasswordRegex();
        private static readonly Regex DigitRegex = new Regex(@"[0-9]", RegexOptions.Compiled);

        /// <summary>
        /// Boolean representation for the SanitizeLogEntry setting. True if SanitizeLogEntry is not in the configuration.
        /// </summary>
        /// <returns>false if ORB has been configured to log private data, true otherwise</returns>
        private static bool GetShouldSanitize()
        {
            var setting = System.Configuration.ConfigurationManager.AppSettings["SanitizeLogEntry"];
            bool should;
            bool configured = bool.TryParse(setting, out should);
            return !configured || should;
        }

        /// <summary>
        /// Get the configuration for the tax ID regular epxression
        /// </summary>
        /// <returns>String that contains the regular expression that should match tax IDs</returns>
        private static Regex GetTaxIdRegex()
        {
            string setting = System.Configuration.ConfigurationManager.AppSettings["TaxIdRegex"];
            if (setting.IsNullOrEmpty())
            {
                // not configured so using default pattern
                setting = @"\b([0-9]{1,3})([ -]?)([0-9]{2,3})([ -]?)([0-9]{4})([a-zA-Z]?)\b";
            }
            return new Regex(setting, RegexOptions.Compiled);
        }

        /// <summary>
        /// Get the configuration for the private number regular expression
        /// </summary>
        /// <returns>String that contains the regular expression that should match private numbers</returns>
        private static Regex GetPrivateNumberRegex()
        {
            string setting = System.Configuration.ConfigurationManager.AppSettings["PrivateNumberRegex"];
            if (setting.IsNullOrEmpty())
            {
                // not configured so using default pattern
                setting = @"\b(?<![0-9\*\-][:\s\./]?)([0-9]{2,19})(?![:\s\./]?[0-9\*\-])\b";
            }
            return new Regex(setting, RegexOptions.Compiled);
        }

        /// <summary>
        /// Configuration for ConnStr Password Regex
        /// </summary>
        /// <returns>The regular expression that should match passwords in connection strings</returns>
        private static Regex GetConnectionPasswordRegex()
        {
            var setting = System.Configuration.ConfigurationManager.AppSettings["ConnectionPasswordRegex"];
            if (setting.IsNullOrEmpty())
            {
                // not configured so default should be used
                setting = "[Pp]assword=[^,]+";
            }
            return new Regex(setting, RegexOptions.Compiled);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="input"> the input string we will be masking </param>
        /// <param name="unmaskedTrailingCharacterCount"> the number of characters that will remain unmasked after the operation completes. Defaults to 4.</param>
        /// <param name="maskToLength"> when true this parameter will ensure the length of the masked output is the same length as the input string. When false this setting will put four asterisks in front of the unmasked characters. Defaults to true. </param>
        /// <returns></returns>
        public static string ToMaskedString(this string input, int unmaskedTrailingCharacterCount = 4, bool maskToLength = true)
        {
            if (string.IsNullOrEmpty(input) || input.Length < unmaskedTrailingCharacterCount)
                return input;

            var sb = new StringBuilder();
            var numberOfStars = maskToLength ? input.Length - unmaskedTrailingCharacterCount : 4;
            for (var i = 0; i < numberOfStars; i++)
            {
                sb.Append("*");
            }
            sb.Append(input.Substring(input.Length - unmaskedTrailingCharacterCount, unmaskedTrailingCharacterCount));
            return sb.ToString();
        }

        /// <summary>
        /// Redact password from connection string
        /// </summary>
        /// <param name="input">String to sanitize</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeConnStr(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return ConnectionPasswordRegex.Replace(input, "password=**REDACTED**");
        }

        /// <summary>
        /// Mask or remove sensitive data before logging to file, database, etc.
        /// </summary>
        /// <param name="input">The string that should be sanitized</param>
        /// <returns>A string safe for logging</returns>
        public static string SanitizeData(this string input)
        {
            if (ShouldNotSanitize || String.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return input.MaskCardNumbers().RedactTaxId().RedactPrivateNumber();
        }

        /// <summary>
        /// Masks card numbers with stars (*) leaving the last 4 and first 2 digits
        /// </summary>
        /// <param name="input">The string with card numbers to mask</param>
        /// <returns>A string where card numbers have been masked</returns>
        public static string MaskCardNumbers(this string input)
        {
            //unifying with CreditCardValidator so this logic isn't in two places (slightly different)
            //Changes in behavior: spaces/dashes can't just be anywhere, will redact even if the number fails luhn
            return CreditCardValidator.RedactCreditCardNumbers(input);
        }

        /// <summary>
        /// Redacts the TaxID based on a pattern.
        /// </summary>
        /// <param name="str">The string to search for the pattern to redact.</param>
        /// <returns>A string where the tax ID has been masked with *'s leaving the last 4 digits</returns>
        private static string RedactTaxId(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }
            // leave in only the last 4 digits
            string asteriskGroup1 = DigitRegex.Replace(TaxIdRegex.Match(str).Groups[1].Value, "*");
            string asteriskGroup2 = DigitRegex.Replace(TaxIdRegex.Match(str).Groups[3].Value, "*");
            return TaxIdRegex.Replace(str, asteriskGroup1 + "$2" + asteriskGroup2 + "$4$5");
        }

        /// <summary>
        /// Redacts a number which represents NPPI based on a patern.
        /// </summary>
        /// <param name="input">The string to search for the pattern to redact.</param>
        /// <returns>A string where the number has been masked with *'s possibly leaving the last 4 digits</returns>
        private static string RedactPrivateNumber(this string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }
            var output = input;
            var matches = PrivateNumberRegex.Matches(input);
            var charactersAdded = 0;
            foreach (Match match in matches)
            {
                // sanitize each number individually
                string stars = DigitRegex.Replace(match.Value, "*");
                string redacted = "****";// numbers that have 2 or 3 digits all get masked as 4 digit numbers
                if (match.Length > 4)
                {
                    // don't mask the last 4 digits
                    redacted = stars.Left(stars.Length - 4) + match.Value.Right(4);
                }
                var index = match.Index + charactersAdded;
                output = output.Substring(0, index) + redacted + output.Substring(index + match.Value.Length);
                charactersAdded += redacted.Length - match.Value.Length;
            }
            return output;
        }

        /// <summary>
        /// Returns the first specified number of characters from a string
        /// </summary>
        /// <param name="text">The string from which to return the first specified number of characters</param>
        /// <param name="length">The number of characters, at maximum to return</param>
        /// <returns>The first specified number of characters</returns>
        /// <remarks>
        /// As String.Substring() will thrown an exception if the string has fewer characters than requested by its
        /// index property, this is a more usable version that mimics the Visual Basic Left() functionality
        /// </remarks>
        public static string Left(this string text, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, $"The parameter, {nameof(length)}, cannot be less than zero.");

            if (length == 0 || string.IsNullOrEmpty(text))
                return (string.Empty);

            return (text.Length <= length ? text : text.Substring(0, length));
        }

        /// <summary>
        /// Returns the last specified number of characters from a string
        /// </summary>
        /// <param name="text">The string from which to return the last specified number of characters</param>
        /// <param name="length">The number of characters, at maximum to return</param>
        /// <returns>The last specified number of characters</returns>
        /// <remarks>
        /// As String.Substring() will thrown an exception if the string has fewer characters than requested by its
        /// index property, this is a more usable version that mimics the Visual Basic Left() functionality
        /// </remarks>
        public static string Right(this string text, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, $"The parameter, {nameof(length)}, cannot be less than zero.");

            if (length == 0 || string.IsNullOrEmpty(text))
                return (string.Empty);

            return (text.Length <= length ? text : text.Substring(text.Length - length, length));
        }

        /// <summary>
        /// Mask or remove sensitive data before logging to file, database, etc.
        /// </summary>
        /// <param name="input">The number that should be sanitized</param>
        /// <returns>A string safe for logging</returns>
        public static string SanitizeData(this long? input)
        {
            if (!input.HasValue)
            {
                return String.Empty;
            }
            return input.Value.SanitizeData();
        }

        /// <summary>
        /// Mask or remove sensitive data before logging to file, database, etc.
        /// </summary>
        /// <param name="input">The number that should be sanitized</param>
        /// <returns>A string safe for logging</returns>
        public static string SanitizeData(this long input)
        {
            return input.ToString().SanitizeData();
        }

        /// <summary>
        /// Mask or remove sensitive data before logging to file, database, etc.
        /// </summary>
        /// <param name="input">The number that should be sanitized</param>
        /// <returns>A string safe for logging</returns>
        public static string SanitizeData(this int input)
        {
            return input.ToString().SanitizeData();
        }

        /// <summary>
        /// Mask an email address.
        /// </summary>
        /// <param name="originalEmailAddress">Email address to be masked</param>
        /// <param name="min">Minimum characters that will be displayed</param>
        /// <param name="max">Maximum characters that will be displayed</param>
        /// <param name="defaultNumOfMasks">Default number of masks</param>
        /// <returns>Masked email address</returns>
        public static string MaskEmailAddress(this string originalEmailAddress, int min = 1, int max = 3, int defaultNumOfMasks = 4)
        {
            if (string.IsNullOrWhiteSpace(originalEmailAddress))
                return string.Empty;

            int t = originalEmailAddress.IndexOf('@');
            if (t <= 0)
                return string.Empty;

            string ToBeMasked = originalEmailAddress.Substring(0, t);
            int unmasked = 0;
            if (ToBeMasked.Length <= min + defaultNumOfMasks)
                unmasked = min;
            else if (ToBeMasked.Length > min + defaultNumOfMasks && ToBeMasked.Length <= max + defaultNumOfMasks)
                unmasked = ToBeMasked.Length - defaultNumOfMasks;
            else
                unmasked = max;

            StringBuilder masked = new StringBuilder();
            int maxStars = 3;
            int maxNumOfLastChars = 2;

            for (int i = 0; i < ToBeMasked.Length - unmasked; i++)
            {
                if (masked.Length < maxStars)
                {
                    masked.Append('*');
                }
            }

            int numOfLastChars = maxNumOfLastChars <= ToBeMasked.Length - unmasked - masked.Length ? maxNumOfLastChars : ToBeMasked.Length - unmasked - masked.Length;

            string lastCharacters = numOfLastChars <= 0 ? "" : ToBeMasked.Substring(ToBeMasked.Length - numOfLastChars, numOfLastChars);

            return originalEmailAddress.Substring(0, unmasked) + masked.ToString() + lastCharacters + originalEmailAddress.Substring(t);

        }

        /// <summary>
        /// Mask a phone number.
        /// </summary>
        /// <param name="phoneNumber">phone Number to be masked</param>
        /// <param name="digitDisplayCount">The number of numbers to show towards the end of the mask</param>
        /// <param name="delimeter">The delimiter used in displaying the phone number</param>
        /// <returns>Masked Phone Number</returns>
        public static string MaskPhoneNumber(this string phoneNumber, int digitDisplayCount = 3, char delimeter = '-')
        {
            var builder = new StringBuilder();

            foreach (char c in phoneNumber)
            {
                if (Char.IsDigit(c))
                    builder.Append(c);
            }

            string digitsOnly = builder.ToString();
            string maskedNumber = new string('*', PHONE_NUMBER_LENGTH - digitDisplayCount) +
                digitsOnly.Right(digitDisplayCount);

            builder.Length = 0;

            builder.Append("(")
                .Append(maskedNumber.Substring(0, 3))
                .Append(") ")
                .Append(maskedNumber.Substring(3, 3))
                .Append(delimeter)
                .Append(maskedNumber.Substring(6));

            return builder.ToString();
        }
    }
}