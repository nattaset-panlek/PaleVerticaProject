#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Globalization;

namespace ScenarioTesting
{
    public static class ScenarioExpression
    {
        #region [Public Methods]
        public static bool TryEvaluate(string expression, Func<string, float?> resolve, out float value, out string error)
        {
            value = 0f;
            error = null;
            if (string.IsNullOrWhiteSpace(expression))
            {
                error = "empty expression";
                return false;
            }

            var parser = new Parser(expression, resolve);
            try
            {
                value = parser.Expression();
                parser.SkipSpaces();
                if (!parser.AtEnd) throw new FormatException($"unexpected '{parser.Rest}'");
                return true;
            }
            catch (FormatException exception)
            {
                error = $"{exception.Message} in \"{expression}\"";
                return false;
            }
        }
        #endregion

        #region [Private Methods]
        private sealed class Parser
        {
            private readonly string _text;
            private readonly Func<string, float?> _resolve;
            private int _index;

            public Parser(string text, Func<string, float?> resolve)
            {
                _text = text;
                _resolve = resolve;
            }

            public bool AtEnd => _index >= _text.Length;
            public string Rest => _text.Substring(_index);

            public float Expression()
            {
                float value = Term();
                while (true)
                {
                    SkipSpaces();
                    if (Match('+')) value += Term();
                    else if (Match('-')) value -= Term();
                    else return value;
                }
            }

            public void SkipSpaces()
            {
                while (!AtEnd && char.IsWhiteSpace(_text[_index])) _index++;
            }

            private float Term()
            {
                float value = Factor();
                while (true)
                {
                    SkipSpaces();
                    if (Match('*')) value *= Factor();
                    else if (Match('/'))
                    {
                        float divisor = Factor();
                        value = divisor == 0f ? 0f : value / divisor;
                    }
                    else return value;
                }
            }

            private float Factor()
            {
                SkipSpaces();
                if (Match('-')) return -Factor();
                if (Match('('))
                {
                    float inner = Expression();
                    SkipSpaces();
                    if (!Match(')')) throw new FormatException("missing ')'");
                    return inner;
                }

                if (AtEnd) throw new FormatException("missing value");
                char first = _text[_index];
                if (char.IsDigit(first) || first == '.') return Number();
                if (char.IsLetter(first) || first == '_') return Name();
                throw new FormatException($"unexpected '{first}'");
            }

            private float Number()
            {
                int start = _index;
                while (!AtEnd && (char.IsDigit(_text[_index]) || _text[_index] == '.')) _index++;
                return float.Parse(_text.Substring(start, _index - start), CultureInfo.InvariantCulture);
            }

            private float Name()
            {
                int start = _index;
                while (!AtEnd && (char.IsLetterOrDigit(_text[_index]) || _text[_index] == '_' || _text[_index] == '.')) _index++;
                string name = _text.Substring(start, _index - start);
                float? value = _resolve(name);
                if (!value.HasValue) throw new FormatException($"unknown value '{name}'");
                return value.Value;
            }

            private bool Match(char character)
            {
                if (AtEnd || _text[_index] != character) return false;
                _index++;
                return true;
            }
        }
        #endregion
    }
}
#endif
