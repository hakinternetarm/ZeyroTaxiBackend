using System.Text.RegularExpressions;

namespace Taxi_API.Services
{
    public static class PhoneNumberValidator
    {
        // Simple E.164-ish validation: optional leading +, then 7-15 digits, first digit not 0
        private static readonly Regex _e164 = new("^\\+?[1-9][0-9]{6,14}$", RegexOptions.Compiled);

        // Returns normalized phone (trimmed) if valid, otherwise null
        public static string? Normalize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var p = phone.Trim();
            // remove spaces, dashes, parentheses
            p = Regex.Replace(p, "[()\\s-]", "");
            if (_e164.IsMatch(p)) return p;
            return null;
        }

        public static bool IsValid(string? phone) => Normalize(phone) != null;
    }
}
