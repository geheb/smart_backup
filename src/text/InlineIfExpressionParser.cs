using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace geheb.smart_backup.text
{
    /// <summary>
    /// simple parser for inline if: {condition ? first_expression : second_expression}
    /// allowed equality operators: <, >, =
    /// </summary>
    sealed class InlineIfExpressionParser
    {
        readonly string _originalArgs;
        readonly int _index;
        readonly int _length;
        readonly string _conditionVar;
        readonly string _equality;
        readonly long _conditionValue;
        readonly string _then;
        readonly string _else;

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
            string param;
            if (conditionVar != _conditionVar)
            {
                param = _else;
            }
            else
            {
                switch (_equality)
                {
                    case "<": param = value < _conditionValue ? _then : _else; break;
                    case ">": param = value > _conditionValue ? _then : _else; break;
                    case "=": param = value == _conditionValue ? _then : _else; break;
                    default: param = _else; break;
                }
            }
            return _originalArgs.Substring(0, _index) + param + _originalArgs.Substring(_index + _length);
        }

    }
}
