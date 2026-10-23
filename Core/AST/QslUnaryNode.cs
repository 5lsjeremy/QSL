using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;
using QSL.Core.Lexer;

namespace QSL.Core.Ast
{
    public sealed class QslUnaryNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Unary;

        public QslTokenKind OperatorKind { get; }
        public IQslNode Operand { get; }

        public QslUnaryNode(
            QslTokenKind operatorKind,
            IQslNode operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }
    }
}