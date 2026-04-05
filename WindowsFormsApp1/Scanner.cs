using System;
using System.Collections.Generic;

namespace TextEditor
{
    public class Scanner
    {
        private string _sourceCode;
        private int _currentPos;
        private int _currentLine;
        private int _lineStartPos;
        private List<Token> _tokens;
        private List<Token> _errors;

        public static readonly Dictionary<string, TokenTypeInfo> TokenTypes = new Dictionary<string, TokenTypeInfo>
        {
            { "while", new TokenTypeInfo(101, "Ключевое слово while") },
            { "do", new TokenTypeInfo(102, "Ключевое слово do") },
            { "for", new TokenTypeInfo(103, "Ключевое слово for") },
            { "if", new TokenTypeInfo(104, "Ключевое слово if") },
            { "then", new TokenTypeInfo(105, "Ключевое слово then") },
            { "else", new TokenTypeInfo(106, "Ключевое слово else") },
            { "let", new TokenTypeInfo(107, "Ключевое слово let") },
            { "in", new TokenTypeInfo(108, "Ключевое слово in") },
            { "match", new TokenTypeInfo(109, "Ключевое слово match") },
            { "with", new TokenTypeInfo(110, "Ключевое слово with") },
            { "function", new TokenTypeInfo(111, "Ключевое слово function") },
            { "fun", new TokenTypeInfo(112, "Ключевое слово fun") },
            { "rec", new TokenTypeInfo(113, "Ключевое слово rec") },
            { "module", new TokenTypeInfo(114, "Ключевое слово module") },
            { "open", new TokenTypeInfo(115, "Ключевое слово open") },
            { "type", new TokenTypeInfo(116, "Ключевое слово type") },
            
            { "ID", new TokenTypeInfo(117, "Идентификатор") },
            
            { "NUM", new TokenTypeInfo(202, "Целое число") },
            { "FLOAT", new TokenTypeInfo(203, "Вещественное число") },
            
            { "<", new TokenTypeInfo(301, "Оператор меньше") },
            { ">", new TokenTypeInfo(302, "Оператор больше") },
            { "<=", new TokenTypeInfo(303, "Оператор меньше или равно") },
            { ">=", new TokenTypeInfo(304, "Оператор больше или равно") },
            { "=", new TokenTypeInfo(305, "Оператор равно") },
            { "<>", new TokenTypeInfo(306, "Оператор не равно") },
            
            { "+", new TokenTypeInfo(307, "Оператор сложения") },
            { "-", new TokenTypeInfo(308, "Оператор вычитания") },
            { "*", new TokenTypeInfo(309, "Оператор умножения") },
            { "/", new TokenTypeInfo(310, "Оператор деления") },
            { "%", new TokenTypeInfo(311, "Оператор остатка от деления") },
            
            { "&&", new TokenTypeInfo(312, "Оператор логическое И") },
            { "||", new TokenTypeInfo(313, "Оператор логическое ИЛИ") },
            { "not", new TokenTypeInfo(314, "Оператор логическое НЕ") },
            
            { "::", new TokenTypeInfo(315, "Оператор cons") },
            { "->", new TokenTypeInfo(316, "Оператор стрелка") },
            { "<-", new TokenTypeInfo(317, "Оператор присваивания") },
            { ".", new TokenTypeInfo(318, "Оператор доступа к члену") },
            
            { ";", new TokenTypeInfo(401, "Разделитель (точка с запятой)") },
            { ",", new TokenTypeInfo(402, "Разделитель (запятая)") },
            { ":", new TokenTypeInfo(403, "Разделитель (двоеточие)") },
            
            { "(", new TokenTypeInfo(501, "Открывающая скобка") },
            { ")", new TokenTypeInfo(502, "Закрывающая скобка") },
            { "[", new TokenTypeInfo(503, "Открывающая квадратная скобка") },
            { "]", new TokenTypeInfo(504, "Закрывающая квадратная скобка") },
            { "{", new TokenTypeInfo(505, "Открывающая фигурная скобка") },
            { "}", new TokenTypeInfo(506, "Закрывающая фигурная скобка") },
            { "|", new TokenTypeInfo(507, "Вертикальная черта") },
            
            { "ERR", new TokenTypeInfo(-1, "Недопустимый символ") }
        };

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "while", "do", "for", "if", "then", "else", "let", "in",
            "match", "with", "function", "fun", "rec", "module", "open", "type", "not"
        };

        private static readonly HashSet<string> TwoCharOperators = new HashSet<string>
        {
            "<=", ">=", "<>", "->", "<-", "::", "&&", "||"
        };

        public Scanner()
        {
        }

        public (List<Token> Tokens, List<Token> Errors) Scan(string sourceCode)
        {
            _sourceCode = sourceCode ?? "";
            _currentPos = 0;
            _currentLine = 1;
            _lineStartPos = 0;
            _tokens = new List<Token>();
            _errors = new List<Token>();

            while (_currentPos < _sourceCode.Length)
            {
                SkipWhitespaceAndNewlines();
                if (_currentPos >= _sourceCode.Length) break;

                int startPos = _currentPos;
                int startLine = _currentLine;
                int startCharPos = startPos - _lineStartPos;

                Token token = GetNextToken();
                if (token != null)
                {
                    token.Line = startLine;
                    token.StartPos = startCharPos;
                    token.EndPos = (_currentPos - 1) - _lineStartPos;
                    _tokens.Add(token);
                    if (token.Code == -1)
                    {
                        _errors.Add(token);
                    }
                }
                else
                {
                    char invalidChar = _sourceCode[_currentPos];
                    Token errorToken = new Token
                    {
                        TypeInfo = TokenTypes["ERR"],
                        Value = invalidChar.ToString(),
                        Line = _currentLine,
                        StartPos = startCharPos,
                        EndPos = startCharPos
                    };
                    _errors.Add(errorToken);
                    _tokens.Add(errorToken);
                    _currentPos++;
                }
            }

            return (_tokens, _errors);
        }

        private void SkipWhitespaceAndNewlines()
        {
            while (_currentPos < _sourceCode.Length)
            {
                char c = _sourceCode[_currentPos];
                if (c == '\n')
                {
                    _currentLine++;
                    _lineStartPos = _currentPos + 1;
                    _currentPos++;
                }
                else if (c == '\r')
                {
                    _currentPos++;
                }
                else if (char.IsWhiteSpace(c))
                {
                    _currentPos++;
                }
                else
                {
                    break;
                }
            }
        }

        private string CheckTwoCharOperator()
        {
            if (_currentPos + 1 >= _sourceCode.Length)
                return null;
            string twoChars = _sourceCode.Substring(_currentPos, 2);
            if (TwoCharOperators.Contains(twoChars))
            {
                return twoChars;
            }
            return null;
        }

        private Token GetNextToken()
        {
            char currentChar = _sourceCode[_currentPos];

            string twoCharOp = CheckTwoCharOperator();
            if (twoCharOp != null)
            {
                _currentPos += 2;
                return new Token { TypeInfo = TokenTypes[twoCharOp], Value = twoCharOp };
            }

            string singleChar = currentChar.ToString();
            if (TokenTypes.ContainsKey(singleChar))
            {
                _currentPos++;
                return new Token { TypeInfo = TokenTypes[singleChar], Value = singleChar };
            }

            if (char.IsDigit(currentChar))
            {
                int start = _currentPos;
                while (_currentPos < _sourceCode.Length && char.IsDigit(_sourceCode[_currentPos]))
                {
                    _currentPos++;
                }

                if (_currentPos < _sourceCode.Length && _sourceCode[_currentPos] == '.')
                {
                    _currentPos++;
                    if (_currentPos < _sourceCode.Length && char.IsDigit(_sourceCode[_currentPos]))
                    {
                        while (_currentPos < _sourceCode.Length && char.IsDigit(_sourceCode[_currentPos]))
                        {
                            _currentPos++;
                        }
                        string floatValue = _sourceCode.Substring(start, _currentPos - start);
                        return new Token { TypeInfo = TokenTypes["FLOAT"], Value = floatValue };
                    }
                    else
                    {
                        _currentPos = start;
                        while (_currentPos < _sourceCode.Length && char.IsDigit(_sourceCode[_currentPos]))
                        {
                            _currentPos++;
                        }
                        string numValue = _sourceCode.Substring(start, _currentPos - start);
                        return new Token { TypeInfo = TokenTypes["NUM"], Value = numValue };
                    }
                }

                string numValue2 = _sourceCode.Substring(start, _currentPos - start);
                return new Token { TypeInfo = TokenTypes["NUM"], Value = numValue2 };
            }

            if (char.IsLetter(currentChar) || currentChar == '_')
            {
                int start = _currentPos;
                while (_currentPos < _sourceCode.Length &&
                       (char.IsLetterOrDigit(_sourceCode[_currentPos]) || _sourceCode[_currentPos] == '_'))
                {
                    _currentPos++;
                }
                string word = _sourceCode.Substring(start, _currentPos - start);
                if (Keywords.Contains(word))
                {
                    return new Token { TypeInfo = TokenTypes[word], Value = word };
                }
                else
                {
                    return new Token { TypeInfo = TokenTypes["ID"], Value = word };
                }
            }

            return null;
        }
    }

    public class TokenTypeInfo
    {
        public int Code { get; set; }
        public string Description { get; set; }

        public TokenTypeInfo(int code, string description)
        {
            Code = code;
            Description = description;
        }
    }

    public class Token
    {
        public TokenTypeInfo TypeInfo { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }

        public int Code => TypeInfo?.Code ?? -1;
        public string Type => TypeInfo?.Description ?? "Ошибка";
        public string Position => $"{Line}:{StartPos}";
    }
}