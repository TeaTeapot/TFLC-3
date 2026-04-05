using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _currentTokenIndex;
        private List<SyntaxError> _errors;
        private string _sourceText;
        private int _errorCount;

        public Parser()
        {
        }

        public List<SyntaxError> Parse(List<Token> tokens, string sourceText)
        {
            _tokens = tokens;
            _sourceText = sourceText;
            _currentTokenIndex = 0;
            _errors = new List<SyntaxError>();
            _errorCount = 0;

            try
            {
                if (_tokens.Count == 0)
                {
                    AddError("", "EOF", "Пустая строка. Ожидалось ключевое слово 'while'", -1, -1, 1);
                    return _errors;
                }

                ParseWhileLoop();

                if (_currentTokenIndex < _tokens.Count)
                {
                    var unexpectedToken = _tokens[_currentTokenIndex];
                    string fragment = unexpectedToken.Value;
                    string location = $"{unexpectedToken.Line}:{unexpectedToken.StartPos}";
                    string description = $"Неожиданная лексема после завершения цикла: '{fragment}'";
                    int charPosition = GetCharPosition(unexpectedToken);
                    AddError(fragment, location, description, _currentTokenIndex, charPosition, unexpectedToken.Line);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(new SyntaxError("Критическая ошибка", "", ex.Message, -1, -1, -1));
            }

            return _errors;
        }

        private void ParseWhileLoop()
        {
            if (_currentTokenIndex >= _tokens.Count)
            {
                AddError("", "EOF", "Ожидалось ключевое слово 'while' в начале строки", -1, -1, 1);
                return;
            }

            var firstToken = _tokens[_currentTokenIndex];
            if (firstToken.Code != TokenCodes.WhileKeyword)
            {
                string fragment = firstToken.Value;
                string location = $"{firstToken.Line}:{firstToken.StartPos}";
                string description = $"Ожидалось ключевое слово 'while', найдено '{fragment}'";
                int charPosition = GetCharPosition(firstToken);
                AddError(fragment, location, description, _currentTokenIndex, charPosition, firstToken.Line);

                _currentTokenIndex++;

                ParseConditionWithoutWhile();
                ParseBody();
                return;
            }

            Consume(TokenCodes.WhileKeyword, "Ожидалось ключевое слово 'while'");
            if (HasErrorInCurrentRule()) return;

            ParseCondition();
            ParseBody();
        }

        private void ParseConditionWithoutWhile()
        {
            ParseLogicalOrExpression();

            if (_currentTokenIndex < _tokens.Count && _tokens[_currentTokenIndex].Code == TokenCodes.DoKeyword)
            {
                Consume(TokenCodes.DoKeyword, "Ожидалось ключевое слово 'do'");
            }
            else
            {
                AddError("", "EOF", "Ожидалось ключевое слово 'do'", _currentTokenIndex, -1, -1);
            }
        }

        private void ParseCondition()
        {
            ParseLogicalOrExpression();
            Consume(TokenCodes.DoKeyword, "Ожидалось ключевое слово 'do'");
        }

        private void ParseLogicalOrExpression()
        {
            ParseLogicalAndExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Or)
                {
                    _currentTokenIndex++;
                    ParseLogicalAndExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseLogicalAndExpression()
        {
            ParseEqualityExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.And)
                {
                    _currentTokenIndex++;
                    ParseEqualityExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseEqualityExpression()
        {
            ParseRelationalExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Equal || token.Code == TokenCodes.NotEqual)
                {
                    _currentTokenIndex++;
                    ParseRelationalExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseRelationalExpression()
        {
            ParseAdditiveExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Less || token.Code == TokenCodes.Greater ||
                    token.Code == TokenCodes.LessOrEqual || token.Code == TokenCodes.GreaterOrEqual)
                {
                    _currentTokenIndex++;
                    ParseAdditiveExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseAdditiveExpression()
        {
            ParseMultiplicativeExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Add || token.Code == TokenCodes.Sub)
                {
                    _currentTokenIndex++;
                    ParseMultiplicativeExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseMultiplicativeExpression()
        {
            ParseUnaryExpression();

            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Mul || token.Code == TokenCodes.Div || token.Code == TokenCodes.Mod)
                {
                    _currentTokenIndex++;
                    ParseUnaryExpression();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseUnaryExpression()
        {
            if (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (token.Code == TokenCodes.Not)
                {
                    _currentTokenIndex++;
                    ParseUnaryExpression();
                    return;
                }
                if (token.Code == TokenCodes.Add || token.Code == TokenCodes.Sub)
                {
                    _currentTokenIndex++;
                    ParseUnaryExpression();
                    return;
                }
            }
            ParsePrimaryExpression();
        }

        private void ParsePrimaryExpression()
        {
            if (_currentTokenIndex >= _tokens.Count)
            {
                AddError("конец файла", "EOF", "Неожиданный конец файла в выражении", -1, -1, -1);
                return;
            }

            var token = _tokens[_currentTokenIndex];
            if (token.Code == TokenCodes.ID)
            {
                _currentTokenIndex++;
            }
            else if (token.Code == TokenCodes.Num || token.Code == TokenCodes.Float)
            {
                _currentTokenIndex++;
            }
            else if (token.Code == TokenCodes.LeftParen)
            {
                _currentTokenIndex++;
                ParseLogicalOrExpression();
                Consume(TokenCodes.RightParen, "Ожидалась закрывающая скобка ')'");
            }
            else
            {
                string fragment = token.Value;
                string location = $"{token.Line}:{token.StartPos}";
                string description = $"Ожидался идентификатор, число или выражение в скобках, найдено '{fragment}'";
                int charPosition = GetCharPosition(token);
                AddError(fragment, location, description, _currentTokenIndex, charPosition, token.Line);

                Synchronize(new HashSet<int> { TokenCodes.ID, TokenCodes.Num, TokenCodes.Float, TokenCodes.LeftParen });
            }
        }

        private void ParseBody()
        {
            if (_currentTokenIndex >= _tokens.Count)
            {
                AddError("конец файла", "EOF", "Ожидалось тело цикла после 'do'", -1, -1, -1);
                return;
            }

            var token = _tokens[_currentTokenIndex];
            if (token.Code == TokenCodes.Semicolon)
            {
                AddError(token.Value, $"{token.Line}:{token.StartPos}",
                    "Тело цикла не может быть пустым",
                    _currentTokenIndex, GetCharPosition(token), token.Line);
                _currentTokenIndex++;
                return;
            }

            ParseStatement();

            while (_currentTokenIndex < _tokens.Count)
            {
                var nextToken = _tokens[_currentTokenIndex];
                if (nextToken.Code == TokenCodes.Semicolon)
                {
                    _currentTokenIndex++;
                    if (_currentTokenIndex >= _tokens.Count)
                        break;
                    ParseStatement();
                }
                else
                {
                    break;
                }
            }
        }

        private void ParseStatement()
        {
            if (_currentTokenIndex >= _tokens.Count)
                return;

            var token = _tokens[_currentTokenIndex];
            if (token.Code == TokenCodes.ID)
            {
                string idName = token.Value;
                int idPosition = GetCharPosition(token);
                int idLine = token.Line;
                _currentTokenIndex++;

                if (_currentTokenIndex < _tokens.Count && _tokens[_currentTokenIndex].Code == TokenCodes.Assign)
                {
                    _currentTokenIndex++;
                    ParseLogicalOrExpression();
                    Consume(TokenCodes.Semicolon, "Ожидался разделитель ';' в конце оператора");
                }
                else
                {
                    var assignToken = Peek();
                    string fragment = assignToken?.Value ?? "конец файла";
                    string location = assignToken != null ? $"{assignToken.Line}:{assignToken.StartPos}" : "EOF";
                    string description = $"Ожидался оператор присваивания '<-', найдено '{fragment}'";
                    int charPosition = assignToken != null ? GetCharPosition(assignToken) : -1;
                    int line = assignToken?.Line ?? -1;
                    AddError(fragment, location, description, _currentTokenIndex, charPosition, line);

                    Synchronize(new HashSet<int> { TokenCodes.Semicolon, TokenCodes.RightBrace });
                }
            }
            else
            {
                string fragment = token.Value;
                string location = $"{token.Line}:{token.StartPos}";
                string description = $"Ожидался идентификатор для оператора присваивания, найдено '{fragment}'";
                int charPosition = GetCharPosition(token);
                AddError(fragment, location, description, _currentTokenIndex, charPosition, token.Line);

                Synchronize(new HashSet<int> { TokenCodes.Semicolon, TokenCodes.RightBrace });
            }
        }

        private Token Peek()
        {
            if (_currentTokenIndex < _tokens.Count)
                return _tokens[_currentTokenIndex];
            return null;
        }

        private void Consume(int expectedCode, string errorMessage)
        {
            if (_currentTokenIndex < _tokens.Count && _tokens[_currentTokenIndex].Code == expectedCode)
            {
                _currentTokenIndex++;
            }
            else
            {
                var currentToken = Peek();
                string fragment = currentToken?.Value ?? "конец файла";
                string location = currentToken != null ? $"{currentToken.Line}:{currentToken.StartPos}" : "EOF";
                string description = $"{errorMessage}, найдено '{fragment}'";
                int charPosition = currentToken != null ? GetCharPosition(currentToken) : -1;
                int line = currentToken?.Line ?? -1;
                AddError(fragment, location, description, _currentTokenIndex, charPosition, line);

                if (expectedCode != TokenCodes.Semicolon)
                {
                    Synchronize(new HashSet<int> { expectedCode, TokenCodes.DoKeyword, TokenCodes.Semicolon });
                }
            }
        }

        private void Synchronize(HashSet<int> syncTokens)
        {
            while (_currentTokenIndex < _tokens.Count)
            {
                var token = _tokens[_currentTokenIndex];
                if (syncTokens.Contains(token.Code))
                {
                    return;
                }
                _currentTokenIndex++;
            }
        }

        private bool HasErrorInCurrentRule()
        {
            return _errors.Count > 0 && _errors[_errors.Count - 1].TokenIndex == _currentTokenIndex;
        }

        private void AddError(string fragment, string location, string description,
            int tokenIndex, int charPosition, int line)
        {
            _errors.Add(new SyntaxError(fragment, location, description, tokenIndex, charPosition, line));
            _errorCount++;
        }

        private int GetCharPosition(Token token)
        {
            if (string.IsNullOrEmpty(_sourceText) || token == null)
                return -1;

            int pos = 0;
            string[] lines = _sourceText.Split('\n');
            for (int i = 0; i < token.Line - 1 && i < lines.Length; i++)
            {
                pos += lines[i].Length + 1;
            }
            pos += token.StartPos;
            return pos;
        }
    }
}