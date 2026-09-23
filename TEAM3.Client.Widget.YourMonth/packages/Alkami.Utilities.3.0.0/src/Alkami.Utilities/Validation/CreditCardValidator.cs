using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Alkami.Utilities.Validation
{
    /// <summary>
    ///
    /// </summary>
    public static class CreditCardValidator
    {
        //according to wikipedia, there is no standard mandated minimum length for the portion after the issuer, and in fact certain maestro cards are only 12 digits
        //however, from a practical standpoint, a short number is far more likely to be invalid that a valid card from some exotic issuer
        //there are extant examples of people writing short numbers as XXXX XXXX XXXX XXX, but that's a weird enough corner case it's probably not worth checking for.
        //13-19 digits with no formatting, 16 digits with spaces/dashes, or amex special snowflake format
        private static readonly Regex UnifiedCardRe = new Regex("([0-9]{2})" + // first two digits (part of Issuer Identification Number)
                                                                "(" +
                                                                "([0-9]{7,13})|" + //all digits
                                                                "([0-9]{2,2} [0-9]{4,4} [0-9]{4,4} )|" + //spaced quads
                                                                "([0-9]{2,2}-[0-9]{4,4}-[0-9]{4,4}-)|" + //dashed quads
                                                                "([0-9]{2,2}-[0-9]{6,6}-[0-9])" + //amex 4-6-5
                                                              ")" +
                                                              "([0-9]{4,4})"); //last four

        /// <summary>
        /// Sanitize a string with one or more credit card numbers
        /// </summary>
        /// <param name="s">The string with credit card number(s) to redact</param>
        /// <returns>The string with redacted credit card number(s)</returns>
        public static string RedactCreditCardNumbers(string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;
            var sb = new StringBuilder(s);
            var matches = UnifiedCardRe.Matches(s);
            foreach (Match m in matches)
            {
                var sb2 = new StringBuilder();
                // show first 2 digits
                sb2.Append(m.Groups[1].Value);
                foreach (var c in m.Groups[2].Value)
                    sb2.Append(Char.IsDigit(c) ? '*' : c);
                sb2.Append(m.Groups[7].Value);
                sb.Remove(m.Index, m.Length);
                sb.Insert(m.Index, sb2);
            }
            return sb.ToString();
        }

        private static string SquishCardNumber(string s)
        {
            return s.Trim().Replace(" ", "").Replace("-", "");
        }

        /// <summary>
        /// Normalizes the credit card number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">number</exception>
        public static string NormalizeCreditCardNumber(string number)
        {
            if (number == null)
                throw new ArgumentNullException(nameof(number));
            var clean = SquishCardNumber(number);
            if (!IsValidCreditCardNumber(clean))
                throw new FormatException($"{number} is not a valid credit card number");
            return clean;
        }

        /// <summary>
        /// Determines whether [is valid credit card number] [the specified pan].
        /// Does not validate issuer. The list of valid issuers would have to be maintained somehow.
        /// </summary>
        /// <param name="pan">no dashes or spaces.</param>
        /// <returns></returns>
        public static bool IsValidCreditCardNumber(string pan)
        {
            return !String.IsNullOrWhiteSpace(pan) && ValidateLuhn(pan) && UnifiedCardRe.IsMatch(pan);
        }

        /// <summary>
        /// ignores non-digit characters. will not work if string contains no digit characters
        /// </summary>
        /// <param name="digits"></param>
        /// <returns></returns>
        private static bool ValidateLuhn(string digits)
        {
            bool dbl = false;
            int total = 0;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int tmp = (digits[i]) - '0';//implentation defined behavior in C (wouldn't work with ebcdic), but in C# chars are always utf-16 per spec
                if (tmp >= 0 && tmp <= 9)
                {
                    total += ((dbl) ? LuhnDouble(tmp) : tmp);
                    dbl = !dbl;
                }
            }
            return total % 10 == 0;
        }

        private static int LuhnDouble(int d)
        {
            var t = 2 * d;
            return (t > 9) ? t - 9 : t;
        }
    }
}