using QSL.Core.Ast;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslLiteralNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Literal;

        public QslLiteralKind LiteralKind { get; init; }
        public object? Value { get; init; }

        public QslLiteralNode(QslLiteralKind kind, object? value)
        {
            LiteralKind = kind;
            Value = value;
        }
    }
}


