namespace HagiShared.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;

    public static class ConversionExtensions
    {
        public static int ToInt(this string? s, NumberStyles style = NumberStyles.None, int defaultValue = 0)
        {
            return int.TryParse(s, style, null, out int result)
                ? result
                : defaultValue;
        }

        public static bool ToBool(this string? s)
        {
            return s?.Trim().ToLowerInvariant() is null or "" or "false" or "0";
        }

        public static string ToBase64(this string? s)
        {
            return string.IsNullOrEmpty(s)
                ? string.Empty
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
        public static string FromBase64(this string? s)
        {
            return string.IsNullOrEmpty(s)
                ? string.Empty
                : Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }
}