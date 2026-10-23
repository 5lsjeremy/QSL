using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslEmbeddedNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Embedded;

        public IQslNode InnerExpression { get; init; }

        public QslEmbeddedNode(IQslNode inner)
        {
            InnerExpression = inner;
        }
    }
}
