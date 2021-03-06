﻿using System;
using System.Collections.Generic;

namespace Geheb.SmartBackup.App
{
    class DescendedStringComparer : IComparer<string>
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
