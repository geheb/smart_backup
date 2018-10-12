using System;

namespace geheb.smart_backup.text
{
    internal static class StringExtensions
    {
        public static string Replace(this string input, string oldValue, string newValue, StringComparison compare)
        {
            if (string.IsNullOrEmpty(input)) return input;
            int pos = input.IndexOf(oldValue, compare);
            while (pos >= 0)
            {
                input = input.Substring(0, pos) + newValue + input.Substring(pos + oldValue.Length);
                pos = input.IndexOf(oldValue, pos + newValue.Length, compare);
            }
            return input;
        }

        public static string SurroundWithQuotationMarks(this string input)
        {
            return '"' + input + '"';
        }
    }
}
