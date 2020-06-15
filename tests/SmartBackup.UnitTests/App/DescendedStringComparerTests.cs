using Geheb.SmartBackup.App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Geheb.SmartBackup.UnitTests.App
{
    [TestClass]
    public class DescendedStringComparerTests
    {
        private readonly DescendedStringComparer _descendedStringComparer = new DescendedStringComparer(StringComparison.OrdinalIgnoreCase);

        [TestMethod]
        public void Compare_UnsortedValues_ExpectsLessThenZero()
        {
            Assert.IsTrue(0 > _descendedStringComparer.Compare("foo", "bar"));
        }

        [TestMethod]
        public void Compare_SortedValues_ExpectsGreaterThenZero()
        {
            Assert.IsTrue(0 < _descendedStringComparer.Compare("bar", "foo"));
        }

        [TestMethod]
        public void Compare_IdenticalValues_ExpectsZero()
        {
            Assert.AreEqual(0, _descendedStringComparer.Compare("foo", "foo"));
        }

        [TestMethod]
        public void Compare_ValueWithNull_ExpectsLessThenZero()
        {
            Assert.AreEqual(-1, _descendedStringComparer.Compare("foo", null));
        }

        [TestMethod]
        public void Compare_NullWithValue_ExpectsGreatorThenZero()
        {
            Assert.AreEqual(1, _descendedStringComparer.Compare(null, "foo"));
        }

        [TestMethod]
        public void Compare_NullWithNull_ExpectsZero()
        {
            Assert.AreEqual(0, _descendedStringComparer.Compare(null, null));
        }
    }
}
