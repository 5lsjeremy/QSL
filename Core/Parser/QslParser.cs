using System.Collections.Generic;
using System.Globalization;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Ast.Types;
using QSL.Core.Lexer;

namespace QSL.Core.Parser
{
    public sealed class QslParser
    {
        private readonly List<QslToken> _tokens;
        private readonly List<string> _errors =
            new List<string>();

        private int _pos;

        public IReadOnlyList<string> Errors =>
            _errors;

        public QslParser(
            IEnumerable<QslToken> tokens)
        {
            _tokens =
                new List<QslToken>(
                    tokens
                );

            _pos = 0;
        }

        public IQslNode ParseExpression()
        {
            var expression =
                ParseExpressionCore();

            if (Peek().Kind !=
                QslTokenKind.EndOfFile)
            {
                var token =
                    Peek();

                var message =
                    "Unexpected trailing token: " +
                    $"{token.Kind} '{token.Text}'.";

                _errors.Add(
                    message
                );
            }

            return expression;
        }

        private IQslNode ParseExpressionCore()
        {
            var expression =
                ParseBinaryExpression(0);

            // Ternary:
            // condition ? trueExpression : falseExpression
            if (Peek().Kind ==
                QslTokenKind.Question)
            {
                Advance();

                var trueExpression =
                    ParseExpressionCore();

                if (Peek().Kind !=
                    QslTokenKind.Colon)
                {
                    const string message =
                        "Expected ':' in ternary expression.";

                    _errors.Add(
                        message
                    );

                    return new QslErrorNode(
                        message
                    );
                }

                Advance();

                var falseExpression =
                    ParseExpressionCore();

                expression =
                    new QslTernaryNode(
                        expression,
                        trueExpression,
                        falseExpression
                    );
            }

            return expression;
        }

        private IQslNode ParseBinaryExpression(
            int minimumPrecedence)
        {
            var left =
                ParsePrefixExpression();

            while (true)
            {
                var operatorKind =
                    Peek().Kind;

                if (!QslKindExtensions.IsBinary(
                        operatorKind))
                {
                    break;
                }

                var precedence =
                    QslKindExtensions.Precedence(
                        operatorKind
                    );

                if (precedence <
                    minimumPrecedence)
                {
                    break;
                }

                var rightAssociative =
                    QslKindExtensions.RightAssociative(
                        operatorKind
                    );

                Advance();

                var nextMinimumPrecedence =
                    rightAssociative
                        ? precedence
                        : precedence + 1;

                var right =
                    ParseBinaryExpression(
                        nextMinimumPrecedence
                    );

                left =
                    new QslBinaryNode(
                        operatorKind,
                        left,
                        right
                    );
            }

            return left;
        }

        private IQslNode ParsePrefixExpression()
        {
            var operatorKind =
                Peek().Kind;

            if (!QslKindExtensions.IsUnary(
                    operatorKind))
            {
                return ParsePostfixExpression();
            }

            Advance();

            var unaryPrecedence =
                QslKindExtensions.UnaryPrecedence(
                    operatorKind
                );

            var operand =
                ParseBinaryExpression(
                    unaryPrecedence
                );

            return new QslUnaryNode(
                operatorKind,
                operand
            );
        }

        private IQslNode ParsePostfixExpression()
        {
            var expression =
                ParsePrimary();

            while (true)
            {
                if (Peek().Kind ==
                    QslTokenKind.LeftParen)
                {
                    expression =
                        ParseCallExpression(
                            expression
                        );

                    continue;
                }

                if (Peek().Kind ==
                    QslTokenKind.Dot)
                {
                    Advance();

                    if (Peek().Kind !=
                        QslTokenKind.Identifier)
                    {
                        var token =
                            Peek();

                        var message =
                            "Expected member name after '.'. " +
                            $"Found {token.Kind} '{token.Text}'.";

                        _errors.Add(
                            message
                        );

                        return new QslErrorNode(
                            message
                        );
                    }

                    var memberToken =
                        Advance();

                    expression =
                        new QslMemberAccessNode(
                            expression,
                            memberToken.Text
                        );

                    continue;
                }

                break;
            }

            return expression;
        }

        private IQslNode ParsePrimary()
        {
            var token =
                Peek();

            switch (token.Kind)
            {
                case QslTokenKind.Number:
                    return ParseNumberLiteral();

                case QslTokenKind.Identifier:
                    return ParseIdentifier();

                case QslTokenKind.LeftParen:
                    return ParseGroupedExpression();

                case QslTokenKind.LeftBrace:
                    return ParseBlock();

                case QslTokenKind.Dollar:
                    return ParseEmbeddedExpression();

                case QslTokenKind.StringStartDouble:
                    return ParseStringLiteralDouble();

                case QslTokenKind.StringStartSingle:
                    return ParseStringLiteralSingle();

                case QslTokenKind.True:
                case QslTokenKind.False:
                    return ParseBooleanLiteral();

                case QslTokenKind.EndOfFile:
                {
                    const string message =
                        "Unexpected end of QSL expression.";

                    _errors.Add(
                        message
                    );

                    return new QslErrorNode(
                        message
                    );
                }

                default:
                {
                    var message =
                        "Unexpected token: " +
                        $"{token.Kind} '{token.Text}'.";

                    _errors.Add(
                        message
                    );

                    Advance();

                    return new QslErrorNode(
                        message
                    );
                }
            }
        }

        private IQslNode ParseNumberLiteral()
        {
            var token =
                Advance();

            double number;

            if (double.TryParse(
                    token.Text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out number))
            {
                return new QslLiteralNode(
                    QslLiteralKind.Number,
                    number
                );
            }

            var message =
                $"Invalid number literal: '{token.Text}'.";

            _errors.Add(
                message
            );

            return new QslErrorNode(
                message
            );
        }

        private IQslNode ParseIdentifier()
        {
            var identifierToken =
                Consume(
                    QslTokenKind.Identifier,
                    "Expected identifier."
                );

            return new QslIdentifierNode(
                identifierToken.Text
            );
        }

        private IQslNode ParseCallExpression(
            IQslNode target)
        {
            Consume(
                QslTokenKind.LeftParen,
                "Expected '(' after call target."
            );

            var arguments =
                new List<IQslNode>();

            if (Peek().Kind !=
                QslTokenKind.RightParen)
            {
                while (Peek().Kind !=
                       QslTokenKind.EndOfFile)
                {
                    arguments.Add(
                        ParseExpressionCore()
                    );

                    if (Peek().Kind ==
                        QslTokenKind.RightParen)
                    {
                        break;
                    }

                    if (Peek().Kind !=
                        QslTokenKind.Comma)
                    {
                        var token =
                            Peek();

                        var message =
                            "Expected ',' between arguments. " +
                            $"Found {token.Kind} '{token.Text}'.";

                        _errors.Add(
                            message
                        );

                        SynchronizeCallArguments();

                        if (Peek().Kind ==
                            QslTokenKind.RightParen)
                        {
                            break;
                        }

                        if (Peek().Kind ==
                            QslTokenKind.EndOfFile)
                        {
                            break;
                        }
                    }

                    if (Peek().Kind ==
                        QslTokenKind.Comma)
                    {
                        Advance();
                    }
                }
            }

            Consume(
                QslTokenKind.RightParen,
                "Expected ')' after arguments."
            );

            return new QslCallNode(
                target,
                arguments
            );
        }

        private IQslNode ParseGroupedExpression()
        {
            Consume(
                QslTokenKind.LeftParen,
                "Expected '('."
            );

            var expression =
                ParseExpressionCore();

            Consume(
                QslTokenKind.RightParen,
                "Expected ')' after grouped expression."
            );

            return expression;
        }

        private IQslNode ParseEmbeddedExpression()
        {
            Consume(
                QslTokenKind.Dollar,
                "Expected '$' for embedded expression."
            );

            Consume(
                QslTokenKind.LeftParen,
                "Expected '(' after '$'."
            );

            var innerExpression =
                ParseExpressionCore();

            Consume(
                QslTokenKind.RightParen,
                "Expected ')' to close embedded expression."
            );

            return new QslEmbeddedNode(
                innerExpression
            );
        }

        private IQslNode ParseStringLiteralDouble()
        {
            Consume(
                QslTokenKind.StringStartDouble,
                "Expected opening double quote."
            );

            var parts =
                new List<IQslNode>();

            var containsInterpolation =
                false;

            while (Peek().Kind !=
                   QslTokenKind.StringEndDouble &&
                   Peek().Kind !=
                   QslTokenKind.EndOfFile)
            {
                if (Peek().Kind ==
                    QslTokenKind.StringContent)
                {
                    var content =
                        Advance().Text;

                    parts.Add(
                        new QslLiteralNode(
                            QslLiteralKind.String,
                            content
                        )
                    );

                    continue;
                }

                if (Peek().Kind ==
                    QslTokenKind.Dollar)
                {
                    containsInterpolation =
                        true;

                    parts.Add(
                        ParseEmbeddedExpression()
                    );

                    continue;
                }

                var token =
                    Peek();

                var message =
                    "Unexpected token inside " +
                    "double-quoted string: " +
                    $"{token.Kind} '{token.Text}'.";

                _errors.Add(
                    message
                );

                Advance();

                parts.Add(
                    new QslErrorNode(
                        message
                    )
                );
            }
            
            if (Peek().Kind ==
                QslTokenKind.EndOfFile)
            {
                const string message =
                    "Expected closing double quote before end of file.";

                _errors.Add(
                    message
                );

                return new QslErrorNode(
                    message
                );
            }

            Consume(
                QslTokenKind.StringEndDouble,
                "Expected closing double quote."
            );

            if (!containsInterpolation)
            {
                if (parts.Count == 0)
                {
                    return new QslLiteralNode(
                        QslLiteralKind.String,
                        string.Empty
                    );
                }

                if (parts.Count == 1 &&
                    parts[0] is QslLiteralNode literal)
                {
                    return literal;
                }
            }

            return new QslInterpolatedStringNode(
                parts
            );
        }

        private IQslNode ParseStringLiteralSingle()
        {
            Consume(
                QslTokenKind.StringStartSingle,
                "Expected opening single quote."
            );

            var content =
                string.Empty;

            if (Peek().Kind ==
                QslTokenKind.StringContent)
            {
                content =
                    Advance().Text;
            }

            Consume(
                QslTokenKind.StringEndSingle,
                "Expected closing single quote."
            );

            return new QslLiteralNode(
                QslLiteralKind.String,
                content
            );
        }

        private IQslNode ParseBlock()
        {
            Consume(
                QslTokenKind.LeftBrace,
                "Expected '{' to start block."
            );

            var statements =
                new List<IQslNode>();

            while (
                Peek().Kind !=
                    QslTokenKind.RightBrace &&
                Peek().Kind !=
                    QslTokenKind.EndOfFile)
            {
                var statement =
                    ParseExpressionCore();

                statements.Add(
                    statement
                );

                if (Peek().Kind ==
                    QslTokenKind.Semicolon)
                {
                    Advance();
                    continue;
                }

                const string message =
                    "Expected ';' between statements.";

                _errors.Add(
                    message
                );

                statements.Add(
                    new QslErrorNode(
                        message
                    )
                );

                SynchronizeBlock();

                if (Peek().Kind ==
                    QslTokenKind.Semicolon)
                {
                    Advance();
                }
            }

            if (Peek().Kind ==
                QslTokenKind.EndOfFile)
            {
                const string message =
                    "Expected '}' before end of file.";

                _errors.Add(
                    message
                );

                return new QslBlockNode(
                    statements
                );
            }

            Consume(
                QslTokenKind.RightBrace,
                "Expected '}' to close block."
            );

            return new QslBlockNode(
                statements
            );
        }

        private IQslNode ParseBooleanLiteral()
        {
            var token =
                Peek();

            if (token.Kind ==
                QslTokenKind.True)
            {
                Advance();

                return new QslLiteralNode(
                    QslLiteralKind.Boolean,
                    true
                );
            }

            if (token.Kind ==
                QslTokenKind.False)
            {
                Advance();

                return new QslLiteralNode(
                    QslLiteralKind.Boolean,
                    false
                );
            }

            var message =
                "Expected boolean literal. " +
                $"Found {token.Kind} '{token.Text}'.";

            _errors.Add(
                message
            );

            Advance();

            return new QslErrorNode(
                message
            );
        }

        private void SynchronizeCallArguments()
        {
            while (
                Peek().Kind !=
                    QslTokenKind.Comma &&
                Peek().Kind !=
                    QslTokenKind.RightParen &&
                Peek().Kind !=
                    QslTokenKind.EndOfFile)
            {
                Advance();
            }
        }

        private void SynchronizeBlock()
        {
            while (
                Peek().Kind !=
                    QslTokenKind.Semicolon &&
                Peek().Kind !=
                    QslTokenKind.RightBrace &&
                Peek().Kind !=
                    QslTokenKind.EndOfFile)
            {
                Advance();
            }
        }

        private QslToken Peek()
        {
            if (_tokens.Count == 0)
            {
                return new QslToken(
                    QslTokenKind.EndOfFile,
                    string.Empty
                );
            }

            if (_pos < _tokens.Count)
                return _tokens[_pos];

            return _tokens[
                _tokens.Count - 1
            ];
        }

        private QslToken Advance()
        {
            var token =
                Peek();

            if (_pos < _tokens.Count)
                _pos++;

            return token;
        }

        private QslToken Consume(
            QslTokenKind expectedKind,
            string message)
        {
            var token =
                Peek();

            if (token.Kind ==
                expectedKind)
            {
                return Advance();
            }

            _errors.Add(
                $"{message} Found " +
                $"{token.Kind} '{token.Text}'."
            );

            return new QslToken(
                QslTokenKind.Unknown,
                token.Text
            );
        }
    }
}