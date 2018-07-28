using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.text
{
    internal sealed class DescendedStringComparer : IComparer<string>
    {
        readonly StringComparison _comparison;

        public DescendedStringComparer(StringComparison comparison)
        {
            _comparison = comparison;
        }

        public int Compare(string x, string y)
        {
            return 0 - string.Compare(x, y, _comparison);
        }
    }

}
