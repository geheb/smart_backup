using System.Text.RegularExpressions;

namespace geheb.smart_backup.text
{
    /// <summary>
    /// simple parser for inline if: {condition ? first_expression : second_expression}
    /// allowed equality operators: <, >, =
    /// </summary>
    internal sealed class InlineIfExpressionParser
    {
        private readonly string _originalArgs;
        private readonly int _index;
        private readonly int _length;
        private readonly string _conditionVar;
        private readonly string _equality;
        private readonly long _conditionValue;
        private readonly string _then;
        private readonly string _else;

        public static InlineIfExpressionParser Parse(string input)
        {
            var matches = Regex.Matches(input,
                @"{\s*([a-z]+)\s*([<>=])\s*(\d+)\s*\?\s*([a-z0-9-=]+)\s*\:\s*([0-9a-z-=]*)\s*}",
                RegexOptions.IgnoreCase);

            if (matches.Count < 1) return null;

            return new InlineIfExpressionParser(input, matches);
        }

        public string Calc(string conditionVar, long value)
        {
            string param = _else;
            if (conditionVar == _conditionVar)
            {
                switch (_equality)
                {
                    case "<": param = value < _conditionValue ? _then : _else; break;
                    case ">": param = value > _conditionValue ? _then : _else; break;
                    case "=": param = value == _conditionValue ? _then : _else; break;
                }
            }
            return _originalArgs.Substring(0, _index) + param + _originalArgs.Substring(_index + _length);
        }

        private InlineIfExpressionParser(string args, MatchCollection matches)
        {
            _originalArgs = args;

            _index = matches[0].Index;
            _length = matches[0].Length;

            _conditionVar = matches[0].Groups[1].Value;
            _equality = matches[0].Groups[2].Value;
            _conditionValue = long.Parse(matches[0].Groups[3].Value);
            _then = matches[0].Groups[4].Value;
            _else = matches[0].Groups[5].Value;
        }
    }
}
