using geheb.smart_backup.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.text
{
    public class StringExtensionsTests
    {
        [Fact]
        public void Replace_LowerCase_ExpectsUpperCase()
        {
            var value = "foo";
            Assert.Equal("FOO", value.Replace("foo", "FOO", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Replace_LowerCaseAtBegining_ExpectsUpperCaseAtBegining()
        {
            var value = "foobar";
            Assert.Equal("FOObar", value.Replace("foo", "FOO", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Replace_LowerCaseAtBeginingAndAtTheEnd_ExpectsUpperCase()
        {
            var value = "foofoo";
            Assert.Equal("FOOFOO", value.Replace("foo", "FOO", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Replace_NonExistent_ExpectsAsInput()
        {
            var value = "foo";
            Assert.Equal("foo", value.Replace("bar", "BAR", StringComparison.OrdinalIgnoreCase));
        }
    }
}
