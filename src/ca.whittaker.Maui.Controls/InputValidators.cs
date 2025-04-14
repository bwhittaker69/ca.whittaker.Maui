using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ca.whittaker.Maui.Controls
{
    public static class InputValidator
    {
        private static readonly Regex plainTextRegex = new Regex(@"^[\x20-\x7E]+$", RegexOptions.Compiled);
        private static readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex usernameRegex = new Regex(@"^[a-zA-Z0-9-._]+$", RegexOptions.Compiled);
        private static readonly Regex numericRegex = new Regex(@"^-?\d+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex integerRegex = new Regex(@"^-?\d+$", RegexOptions.Compiled);
        private static readonly Regex currencyRegex = new Regex(@"^\$?-?\d{1,3}(,\d{3})*(\.\d+)?$", RegexOptions.Compiled);

        // Filter regex patterns
        private static readonly Regex emailFilterRegex = new Regex(@"[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~.@-]", RegexOptions.Compiled);
        private static readonly Regex usernameFilterRegex = new Regex(@"[^a-zA-Z0-9-._]", RegexOptions.Compiled);
        private static readonly Regex urlFilterRegex = new Regex(@"[^a-zA-Z0-9./:]", RegexOptions.Compiled);
        private static readonly Regex numericFilterRegex = new Regex(@"[^0-9\.-]", RegexOptions.Compiled);
        private static readonly Regex integerFilterRegex = new Regex(@"[^0-9-]", RegexOptions.Compiled);
        private static readonly Regex currencyFilterRegex = new Regex(@"[^0-9\.,\-\$]", RegexOptions.Compiled);
        // For filtering plain text: remove any character not in the allowed ASCII range.
        private static readonly Regex plainTextFilterRegex = new Regex(@"[^\x20-\x7E]", RegexOptions.Compiled);
        // For rich text: filter out control characters.
        private static readonly Regex richTextFilterRegex = new Regex(@"[\p{Cc}]", RegexOptions.Compiled);
        // For single line filtering: remove linebreaks and carriage return characters.
        private static readonly Regex singleLineFilterRegex = new Regex(@"[\r\n]+", RegexOptions.Compiled);

        // Validates plain text ensuring only basic ASCII characters are used.
        public static bool IsValidPlaintext(string text) =>
            !string.IsNullOrEmpty(text) &&
            text.Length >= 5 &&
            plainTextRegex.IsMatch(text);

        // Validates rich text which allows plain text plus emoji and special characters.
        public static bool IsValidRichtext(string text) =>
            !string.IsNullOrEmpty(text) &&
            text.Length >= 5;

        // Applies the email filter then validates email format and plain ASCII characters.
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || email.Length < 5)
                return false;
            //string filteredEmail = FilterEmailFilter(true, email);
            return emailRegex.IsMatch(email) &&
                   email.All(c => c <= 127);
        }

        // Validates URL ensuring it's a valid absolute HTTP/HTTPS address.
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url.Length < 5)
                return false;
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        // Applies the username filter then validates against allowed characters.
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 5)
                return false;
            //string filteredUsername = FilterUsernameFilter(true, username);
            return usernameRegex.IsMatch(username);
        }

        // Validates numeric strings.
        public static bool IsValidNumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            //string filteredNumeric = FilterNumeric(true, text);
            return numericRegex.IsMatch(text);
        }

        // Validates integer strings (no decimals).
        public static bool IsValidInteger(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            //string filteredInteger = FilterInteger(true, text);
            return integerRegex.IsMatch(text);
        }

        // Validates currency strings.
        public static bool IsValidCurrency(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            //string filteredCurrency = FilterCurrency(true, text);
            return currencyRegex.IsMatch(text);
        }

        // Filter methods

        // Removes disallowed characters from email strings.
        public static string FilterEmailFilter(bool filter, string text) =>
            filter ? emailFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Removes disallowed characters from username strings.
        public static string FilterUsernameFilter(bool filter, string text) =>
            filter ? usernameFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters URL string to only allow alphanumeric characters, period, forward slash, and colons.
        public static string FilterUrlFilter(bool filter, string text) =>
            filter ? urlFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters numeric string by removing any characters not digits, period, or minus sign.
        public static string FilterNumeric(bool filter, string text) =>
            filter ? numericFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters integer string by removing any characters not digits or minus sign.
        public static string FilterInteger(bool filter, string text) =>
            filter ? integerFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters currency string by removing any characters not digits, comma, period, minus sign, or '$'.
        public static string FilterCurrency(bool filter, string text) =>
            filter ? currencyFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters plain text by removing any characters not in the allowed ASCII range.
        public static string FilterPlaintext(bool filter, string text) =>
            filter ? plainTextFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters rich text by removing control characters.
        public static string FilterRichtext(bool filter, string text) =>
            filter ? richTextFilterRegex.Replace((text ?? string.Empty), "") : text ?? string.Empty;

        // Filters text to a single line by removing linebreaks and carriage returns.
        public static string FilterSingleLine(bool filter, string text) =>
            filter ? singleLineFilterRegex.Replace(text ?? string.Empty, "") : text ?? string.Empty;

        public static string FilterAllLowercase(bool filter, string text) =>
            filter ? (text ?? string.Empty).ToLower() : text ?? string.Empty;

        public static string FilterAllowWhiteSpace(bool filter, string text) =>
            filter ? (text ?? string.Empty).Replace(" ", "") : text ?? string.Empty;
    }
}
