using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslTernaryNode : QslNode
    {
        public override QslNodeKind Kind =>
            QslNodeKind.Ternary;

        public IQslNode Condition { get; }

        public IQslNode TrueExpr { get; }

        public IQslNode FalseExpr { get; }

        public QslTernaryNode(
            IQslNode condition,
            IQslNode trueExpr,
            IQslNode falseExpr)
        {
            Condition = condition;
            TrueExpr = trueExpr;
            FalseExpr = falseExpr;
        }
    }
}