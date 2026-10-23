using System;
using System.Collections.Generic;
using System.Text;

namespace QSL.Core.Lexer
{
    public sealed class QslTokenizer
    {
        private readonly string _source;
        private int _pos;

        public QslTokenizer(string source)
        {
            _source = source ?? string.Empty;
            _pos = 0;
        }

        public IEnumerable<QslToken> Tokenize()
        {
            while (!IsAtEnd())
            {
                var current = Peek();

                if (char.IsWhiteSpace(current))
                {
                    Advance();
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    yield return ReadIdentifier();
                    continue;
                }

                if (char.IsDigit(current))
                {
                    yield return ReadNumber();
                    continue;
                }

                switch (current)
                {
                    case '"':
                    {
                        foreach (var token in ReadStringDouble())
                            yield return token;

                        continue;
                    }

                    case '\'':
                    {
                        foreach (var token in ReadStringSingle())
                            yield return token;

                        continue;
                    }

                    case '$':
                        yield return MakeToken(
                            QslTokenKind.Dollar,
                            "$");

                        Advance();
                        continue;

                    case '(':
                        yield return MakeToken(
                            QslTokenKind.LeftParen,
                            "(");

                        Advance();
                        continue;

                    case ')':
                        yield return MakeToken(
                            QslTokenKind.RightParen,
                            ")");

                        Advance();
                        continue;

                    case '{':
                        yield return MakeToken(
                            QslTokenKind.LeftBrace,
                            "{");

                        Advance();
                        continue;

                    case '}':
                        yield return MakeToken(
                            QslTokenKind.RightBrace,
                            "}");

                        Advance();
                        continue;

                    case ',':
                        yield return MakeToken(
                            QslTokenKind.Comma,
                            ",");

                        Advance();
                        continue;

                    case ';':
                        yield return MakeToken(
                            QslTokenKind.Semicolon,
                            ";");

                        Advance();
                        continue;
                    
                    case '.':
                        yield return MakeToken(
                            QslTokenKind.Dot,
                            ".");
                        Advance();
                        continue;

                    case '+':
                        yield return MakeToken(
                            QslTokenKind.Plus,
                            "+");

                        Advance();
                        continue;

                    case '-':
                        yield return MakeToken(
                            QslTokenKind.Minus,
                            "-");

                        Advance();
                        continue;

                    case '*':
                        yield return MakeToken(
                            QslTokenKind.Star,
                            "*");

                        Advance();
                        continue;

                    case '/':
                        yield return MakeToken(
                            QslTokenKind.Slash,
                            "/");

                        Advance();
                        continue;

                    case '%':
                        yield return MakeToken(
                            QslTokenKind.Percent,
                            "%");

                        Advance();
                        continue;

                    case '&':
                        if (MatchNext('&'))
                        {
                            yield return MakeToken(
                                QslTokenKind.AndAnd,
                                "&&");

                            continue;
                        }

                        yield return MakeUnknownToken(current);
                        Advance();
                        continue;

                    case '|':
                        if (MatchNext('|'))
                        {
                            yield return MakeToken(
                                QslTokenKind.OrOr,
                                "||");

                            continue;
                        }

                        yield return MakeUnknownToken(current);
                        Advance();
                        continue;

                    case '=':
                        if (MatchNext('='))
                        {
                            yield return MakeToken(
                                QslTokenKind.EqualEqual,
                                "==");

                            continue;
                        }

                        if (MatchNext('>'))
                        {
                            yield return MakeToken(
                                QslTokenKind.Arrow,
                                "=>");

                            continue;
                        }

                        yield return MakeToken(
                            QslTokenKind.Equal,
                            "=");

                        Advance();
                        continue;

                    case '!':
                        if (MatchNext('='))
                        {
                            yield return MakeToken(
                                QslTokenKind.BangEqual,
                                "!=");

                            continue;
                        }

                        yield return MakeToken(
                            QslTokenKind.Bang,
                            "!");

                        Advance();
                        continue;

                    case '<':
                        if (MatchNext('='))
                        {
                            yield return MakeToken(
                                QslTokenKind.LessEqual,
                                "<=");

                            continue;
                        }

                        yield return MakeToken(
                            QslTokenKind.Less,
                            "<");

                        Advance();
                        continue;

                    case '>':
                        if (MatchNext('='))
                        {
                            yield return MakeToken(
                                QslTokenKind.GreaterEqual,
                                ">=");

                            continue;
                        }

                        yield return MakeToken(
                            QslTokenKind.Greater,
                            ">");

                        Advance();
                        continue;

                    case '?':
                        if (MatchNext('?'))
                        {
                            yield return MakeToken(
                                QslTokenKind.QuestionQuestion,
                                "??");

                            continue;
                        }

                        yield return MakeToken(
                            QslTokenKind.Question,
                            "?");

                        Advance();
                        continue;

                    case ':':
                        yield return MakeToken(
                            QslTokenKind.Colon,
                            ":");

                        Advance();
                        continue;

                    default:
                        yield return MakeUnknownToken(current);
                        Advance();
                        continue;
                }
            }

            yield return MakeToken(
                QslTokenKind.EndOfFile,
                string.Empty);
        }

        private bool IsAtEnd()
        {
            return _pos >= _source.Length;
        }

        private char Peek()
        {
            return IsAtEnd()
                ? '\0'
                : _source[_pos];
        }

        private char Advance()
        {
            return IsAtEnd()
                ? '\0'
                : _source[_pos++];
        }

        /// <summary>
        /// Checks whether the character immediately after the current
        /// position matches the expected character.
        ///
        /// When matched, both the current character and the following
        /// character are consumed.
        /// </summary>
        private bool MatchNext(char expected)
        {
            var nextPosition = _pos + 1;

            if (nextPosition >= _source.Length)
                return false;

            if (_source[nextPosition] != expected)
                return false;

            _pos += 2;
            return true;
        }

        private QslToken MakeToken(
            QslTokenKind kind,
            string text)
        {
            return new QslToken(kind, text);
        }

        private QslToken MakeUnknownToken(char value)
        {
            return MakeToken(
                QslTokenKind.Unknown,
                value.ToString());
        }

        private QslToken ReadIdentifier()
        {
            var start = _pos;

            while (!IsAtEnd() &&
                   (char.IsLetterOrDigit(Peek()) ||
                    Peek() == '_'))
            {
                Advance();
            }

            var text = _source.Substring(
                start,
                _pos - start);

            switch (text)
            {
                case "true":
                    return MakeToken(
                        QslTokenKind.True,
                        text);

                case "false":
                    return MakeToken(
                        QslTokenKind.False,
                        text);

                default:
                    return MakeToken(
                        QslTokenKind.Identifier,
                        text);
            }
        }

        private QslToken ReadNumber()
        {
            var start = _pos;

            while (!IsAtEnd() &&
                   char.IsDigit(Peek()))
            {
                Advance();
            }

            if (!IsAtEnd() &&
                Peek() == '.' &&
                HasNextDigit())
            {
                Advance();

                while (!IsAtEnd() &&
                       char.IsDigit(Peek()))
                {
                    Advance();
                }
            }

            var text = _source.Substring(
                start,
                _pos - start);

            return MakeToken(
                QslTokenKind.Number,
                text);
        }

        private bool HasNextDigit()
        {
            var nextPosition = _pos + 1;

            return nextPosition < _source.Length &&
                   char.IsDigit(_source[nextPosition]);
        }

        private IEnumerable<QslToken> ReadStringDouble()
        {
            Advance();

            yield return MakeToken(
                QslTokenKind.StringStartDouble,
                "\""
            );

            var content =
                new StringBuilder();

            while (!IsAtEnd())
            {
                var current =
                    Advance();

                if (current == '"')
                {
                    if (content.Length > 0)
                    {
                        yield return MakeToken(
                            QslTokenKind.StringContent,
                            content.ToString()
                        );
                    }

                    yield return MakeToken(
                        QslTokenKind.StringEndDouble,
                        "\""
                    );

                    yield break;
                }

                if (current == '\\')
                {
                    if (IsAtEnd())
                    {
                        throw new InvalidOperationException(
                            "Unterminated escape sequence in " +
                            "double-quoted string."
                        );
                    }

                    var escaped =
                        Advance();

                    content.Append(
                        ResolveEscapeCharacter(
                            escaped
                        )
                    );

                    continue;
                }

                if (current == '$' &&
                    Peek() == '(')
                {
                    if (content.Length > 0)
                    {
                        yield return MakeToken(
                            QslTokenKind.StringContent,
                            content.ToString()
                        );

                        content.Clear();
                    }

                    Advance();

                    yield return MakeToken(
                        QslTokenKind.Dollar,
                        "$"
                    );

                    yield return MakeToken(
                        QslTokenKind.LeftParen,
                        "("
                    );

                    var embeddedSource =
                        ReadEmbeddedSource();

                    var embeddedTokenizer =
                        new QslTokenizer(
                            embeddedSource
                        );

                    foreach (var token in
                             embeddedTokenizer.Tokenize())
                    {
                        if (token.Kind ==
                            QslTokenKind.EndOfFile)
                        {
                            continue;
                        }

                        yield return token;
                    }

                    yield return MakeToken(
                        QslTokenKind.RightParen,
                        ")"
                    );

                    continue;
                }

                content.Append(
                    current
                );
            }

            throw new InvalidOperationException(
                "Unterminated double-quoted string."
            );
            
            
        }


        private string ReadEmbeddedSource()
        {
            var start =
                _pos;

            var depth =
                1;

            var inDoubleQuotedString =
                false;

            var inSingleQuotedString =
                false;

            var escaped =
                false;

            while (!IsAtEnd())
            {
                var current =
                    Advance();

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (inDoubleQuotedString)
                {
                    if (current == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (current == '"')
                    {
                        inDoubleQuotedString =
                            false;
                    }

                    continue;
                }

                if (inSingleQuotedString)
                {
                    if (current == '\'')
                    {
                        inSingleQuotedString =
                            false;
                    }

                    continue;
                }

                if (current == '"')
                {
                    inDoubleQuotedString =
                        true;

                    continue;
                }

                if (current == '\'')
                {
                    inSingleQuotedString =
                        true;

                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current != ')')
                {
                    continue;
                }

                depth--;

                if (depth != 0)
                {
                    continue;
                }

                var length =
                    (_pos - 1) - start;

                return _source.Substring(
                    start,
                    length
                );
            }

            throw new InvalidOperationException(
                "Unterminated embedded expression " +
                "inside double-quoted string."
            );
        }

        private IEnumerable<QslToken> ReadStringSingle()
        {
            Advance();

            yield return MakeToken(
                QslTokenKind.StringStartSingle,
                "'");

            var content = new StringBuilder();

            while (!IsAtEnd())
            {
                var current = Advance();

                if (current == '\'')
                {
                    yield return MakeToken(
                        QslTokenKind.StringContent,
                        content.ToString());

                    yield return MakeToken(
                        QslTokenKind.StringEndSingle,
                        "'");

                    yield break;
                }

                content.Append(current);
            }

            throw new InvalidOperationException(
                "Unterminated single-quoted string.");
        }

        private char ResolveEscapeCharacter(char escaped)
        {
            switch (escaped)
            {
                case '"':
                    return '"';

                case '\'':
                    return '\'';

                case '\\':
                    return '\\';

                case 'n':
                    return '\n';

                case 'r':
                    return '\r';

                case 't':
                    return '\t';

                case '$':
                    return '$';

                case ')':
                    return ')';

                default:
                    return escaped;
            }
        }
    }
}