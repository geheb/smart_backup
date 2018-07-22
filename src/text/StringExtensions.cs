using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.text
{
    static class StringExtensions
    {
        public static string Replace(this string input, string oldValue, string newValue, StringComparison compare)
        {
            if (string.IsNullOrEmpty(input)) return input;
            int pos = input.IndexOf(oldValue, compare);
            if (pos < 0) return input;
            return input.Substring(0, pos) + newValue + input.Substring(pos + oldValue.Length);
        }

        public static string SurroundWithQuotationMarks(this string input)
        {
            return '"' + input + '"';
        }
    }
}
