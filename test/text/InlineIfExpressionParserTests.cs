using geheb.smart_backup.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.text
{
    public class InlineIfExpressionParserTests
    {
        [Fact]
        public void Parse_InvalidFormat_ExpectsNull()
        {
            var parser = InlineIfExpressionParser.Parse("foo > 0 ? bar : baz");
            Assert.Null(parser);
        }

        [Theory]
        [InlineData(">=")]
        [InlineData("<=")]
        public void Parse_InvalidEquality_ExpectsNull(string equality)
        {
            var parser = InlineIfExpressionParser.Parse("{foo " + equality + " 0 ? bar : baz}");
            Assert.Null(parser);
        }

        [Theory]
        [InlineData("bar", 1)]
        [InlineData("baz", 0)]
        [InlineData("baz", -1)]
        public void ParseAndCalc_GreaterThen_ExpectsCorrectValue(string result, long value)
        {
            var parser = InlineIfExpressionParser.Parse("{foo > 0 ? bar : baz}");
            Assert.Equal(result, parser.Calc("foo", value));
        }

        [Theory]
        [InlineData("baz", 1)]
        [InlineData("baz", 0)]
        [InlineData("bar", -1)]
        public void ParseAndCalc_LessThen_ExpectsCorrectValue(string result, long value)
        {
            var parser = InlineIfExpressionParser.Parse("{foo < 0 ? bar : baz}");
            Assert.Equal(result, parser.Calc("foo", value));
        }

        [Theory]
        [InlineData("bar", 0)]
        [InlineData("baz", 1)]
        [InlineData("baz", -1)]
        public void ParseAndCalc_Equal_ExpectsCorrectValue(string result, long value)
        {
            var parser = InlineIfExpressionParser.Parse("{foo = 0 ? bar : baz}");
            Assert.Equal(result, parser.Calc("foo", value));
        }

        [Fact]
        public void ParseAndCalc_InvalidParam_ExpectsSecondValue()
        {
            var parser = InlineIfExpressionParser.Parse("{foo > 0 ? bar : baz}");
            Assert.Equal("baz", parser.Calc("bar", 1));
        }
    }
}
