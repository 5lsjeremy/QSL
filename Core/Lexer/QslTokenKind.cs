using QSL.Core.Runtime.Operators;

namespace QSL.Core.Lexer
{
    public enum QslTokenKind
    {
        // ===== Core =====

        [QslTokenAttribute(
            Symbol = "",
            Type = "core")]
        Unknown,

        [QslTokenAttribute(
            Symbol = "",
            Type = "core")]
        EndOfFile,

        [QslTokenAttribute(
            Symbol = "",
            Type = "identifier")]
        Identifier,

        [QslTokenAttribute(
            Symbol = "",
            Type = "number")]
        Number,

        // ===== Strings =====

        [QslTokenAttribute(
            Symbol = "\"",
            Type = "string")]
        StringStartDouble,

        [QslTokenAttribute(
            Symbol = "'",
            Type = "string")]
        StringStartSingle,

        [QslTokenAttribute(
            Symbol = "",
            Type = "string")]
        StringContent,

        [QslTokenAttribute(
            Symbol = "\"",
            Type = "string")]
        StringEndDouble,

        [QslTokenAttribute(
            Symbol = "'",
            Type = "string")]
        StringEndSingle,

        // ===== Embedded =====

        [QslTokenAttribute(
            Symbol = "$",
            Type = "punctuation")]
        Dollar,

        // ===== Grouping =====

        [QslTokenAttribute(
            Symbol = "(",
            Type = "punctuation")]
        LeftParen,

        [QslTokenAttribute(
            Symbol = ")",
            Type = "punctuation")]
        RightParen,

        [QslTokenAttribute(
            Symbol = "{",
            Type = "punctuation")]
        LeftBrace,

        [QslTokenAttribute(
            Symbol = "}",
            Type = "punctuation")]
        RightBrace,

        // ===== Arithmetic Operators =====

        [QslTokenAttribute(
            Symbol = "+",
            Type = "operator",
            Unary = true,
            Binary = true,
            Precedence = 12,
            UnaryPrecedence = 15,
            ExecutorType = typeof(QslAddOperator))]
        Plus,

        [QslTokenAttribute(
            Symbol = "-",
            Type = "operator",
            Unary = true,
            Binary = true,
            Precedence = 12,
            UnaryPrecedence = 15,
            ExecutorType = typeof(QslSubtractOperator))]
        Minus,

        [QslTokenAttribute(
            Symbol = "*",
            Type = "operator",
            Binary = true,
            Precedence = 13,
            ExecutorType = typeof(QslMultiplyOperator))]
        Star,

        [QslTokenAttribute(
            Symbol = "/",
            Type = "operator",
            Binary = true,
            Precedence = 13,
            ExecutorType = typeof(QslDivideOperator))]
        Slash,

        [QslTokenAttribute(
            Symbol = "%",
            Type = "operator",
            Binary = true,
            Precedence = 13,
            ExecutorType = typeof(QslModuloOperator))]
        Percent,

        // ===== Logical Operators =====

        [QslTokenAttribute(
            Symbol = "&&",
            Type = "operator",
            Binary = true,
            Precedence = 4,
            EvaluationStrategy = QslEvaluationStrategy.ShortCircuitAnd,
            ExecutorType = typeof(QslAndOperator))]
        AndAnd,

        [QslTokenAttribute(
            Symbol = "||",
            Type = "operator",
            Binary = true,
            Precedence = 3,
            EvaluationStrategy = QslEvaluationStrategy.ShortCircuitOr,
            ExecutorType = typeof(QslOrOperator))]
        OrOr,

        // ===== Equality Operators =====

        [QslTokenAttribute(
            Symbol = "==",
            Type = "operator",
            Binary = true,
            Precedence = 6,
            ExecutorType = typeof(QslEqualOperator))]
        EqualEqual,

        [QslTokenAttribute(
            Symbol = "!=",
            Type = "operator",
            Binary = true,
            Precedence = 6,
            ExecutorType = typeof(QslNotEqualOperator))]
        BangEqual,

        // ===== Separators =====

        [QslTokenAttribute(
            Symbol = ",",
            Type = "punctuation")]
        Comma,

        [QslTokenAttribute(
            Symbol = ";",
            Type = "punctuation")]
        Semicolon,

        // ===== Unary Logical Operators =====

        [QslTokenAttribute(
            Symbol = "!",
            Type = "operator",
            Unary = true,
            UnaryPrecedence = 15,
            ExecutorType = typeof(QslNotOperator))]
        Bang,

        // ===== Comparison Operators =====

        [QslTokenAttribute(
            Symbol = "<",
            Type = "operator",
            Binary = true,
            Precedence = 7,
            ExecutorType = typeof(QslLessOperator))]
        Less,

        [QslTokenAttribute(
            Symbol = "<=",
            Type = "operator",
            Binary = true,
            Precedence = 7,
            ExecutorType = typeof(QslLessEqualOperator))]
        LessEqual,

        [QslTokenAttribute(
            Symbol = ">",
            Type = "operator",
            Binary = true,
            Precedence = 7,
            ExecutorType = typeof(QslGreaterOperator))]
        Greater,

        [QslTokenAttribute(
            Symbol = ">=",
            Type = "operator",
            Binary = true,
            Precedence = 7,
            ExecutorType = typeof(QslGreaterEqualOperator))]
        GreaterEqual,

        // ===== Null Coalescing =====

        [QslTokenAttribute(
            Symbol = "??",
            Type = "operator",
            Binary = true,
            Precedence = 2,
            EvaluationStrategy = QslEvaluationStrategy.NullCoalescing,
            ExecutorType = typeof(QslNullCoalescingOperator))]
        QuestionQuestion,

        // ===== Arrow =====

        [QslTokenAttribute(
            Symbol = "=>",
            Type = "operator",
            Binary = true,
            Precedence = 1,
            RightAssociative = true,
            EvaluationStrategy = QslEvaluationStrategy.Arrow)]
        Arrow,

        // ===== Assignment =====

        [QslTokenAttribute(
            Symbol = "=",
            Type = "operator",
            Binary = true,
            Precedence = 2,
            RightAssociative = true,
            EvaluationStrategy = QslEvaluationStrategy.Assignment)]
        Equal,

        // ===== Ternary =====

        [QslTokenAttribute(
            Symbol = "?",
            Type = "punctuation",
            Precedence = 3)]
        Question,

        [QslTokenAttribute(
            Symbol = ":",
            Type = "punctuation",
            Precedence = 3)]
        Colon,

        // ===== Boolean Literals =====

        [QslTokenAttribute(
            Symbol = "true",
            Type = "literal")]
        True,

        [QslTokenAttribute(
            Symbol = "false",
            Type = "literal")]
        False,

        // ===== Member Access =====
        [QslTokenAttribute(
            Symbol = ".",
            Type = "punctuation")]
        Dot
    }
}