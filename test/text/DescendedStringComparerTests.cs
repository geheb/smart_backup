using geheb.smart_backup.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.text
{
    public class DescendedStringComparerTests
    {
        readonly DescendedStringComparer _descendedStringComparer = new DescendedStringComparer(StringComparison.OrdinalIgnoreCase);

        [Fact]
        public void Compare_UnsortedValues_ExpectsLessThenZero()
        {
            Assert.True(0 > _descendedStringComparer.Compare("foo", "bar"));
        }

        [Fact]
        public void Compare_SortedValues_ExpectsGreaterThenZero()
        {
            Assert.True(0 < _descendedStringComparer.Compare("bar", "foo"));
        }

        [Fact]
        public void Compare_IdenticalValues_ExpectsZero()
        {
            Assert.Equal(0, _descendedStringComparer.Compare("foo", "foo"));
        }

        [Fact]
        public void Compare_ValueWithNull_ExpectsLessThenZero()
        {
            Assert.Equal(-1, _descendedStringComparer.Compare("foo", null));
        }

        [Fact]
        public void Compare_NullWithValue_ExpectsGreatorThenZero()
        {
            Assert.Equal(1, _descendedStringComparer.Compare(null, "foo"));
        }

        [Fact]
        public void Compare_NullWithNull_ExpectsZero()
        {
            Assert.Equal(0, _descendedStringComparer.Compare(null, null));
        }
    }
}
