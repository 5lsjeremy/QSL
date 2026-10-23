using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;
using QSL.Core.Lexer;

namespace QSL.Core.Ast
{
    public sealed class QslBinaryNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Binary;

        public QslTokenKind OperatorKind { get; }
        public IQslNode Left { get; }
        public IQslNode Right { get; }

        public QslBinaryNode(
            QslTokenKind operatorKind,
            IQslNode left,
            IQslNode right)
        {
            OperatorKind = operatorKind;
            Left = left;
            Right = right;
        }
    }
}