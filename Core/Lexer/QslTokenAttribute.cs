using System;
using QSL.Core.Ast.Types;

namespace QSL.Core.Lexer
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class QslTokenAttribute : Attribute
    {
        public string Symbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public bool Unary { get; set; }
        public bool Binary { get; set; }

        /// <summary>
        /// Binding precedence when the token is used
        /// as a binary operator.
        /// </summary>
        public int Precedence { get; set; }

        /// <summary>
        /// Binding precedence when the token is used
        /// as a unary operator.
        /// </summary>
        public int UnaryPrecedence { get; set; }

        /// <summary>
        /// Determines whether the binary operator
        /// associates from right to left.
        /// </summary>
        public bool RightAssociative { get; set; }

        /// <summary>
        /// Runtime implementation responsible for
        /// executing this operator.
        /// </summary>
        public Type? ExecutorType { get; set; }

        /// <summary>
        /// Determines how this operator's operands
        /// are evaluated by the runtime.
        /// </summary>
        public QslEvaluationStrategy EvaluationStrategy { get; set; }
            = QslEvaluationStrategy.Eager;
    }
}