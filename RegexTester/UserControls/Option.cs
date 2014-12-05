using System.Text.RegularExpressions;

namespace Sharomank.RegexTester
{
    internal class Option
    {
        private readonly char _key;
        private readonly RegexOptions _regexOptions;
        private readonly string _tooltip;

        public Option(char key, RegexOptions regexOptions, string tooltip)
        {
            _key = key;
            _regexOptions = regexOptions;
            _tooltip = tooltip;
        }

        public char Key
        {
            get { return _key; }
        }

        public RegexOptions RegexOptions
        {
            get { return _regexOptions; }
        }

        public string Tooltip
        {
            get { return _tooltip; }
        }
    }
}