using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslCallNode : QslNode
    {
        public override QslNodeKind Kind =>
            QslNodeKind.Call;

        public IQslNode Target { get; }

        public IReadOnlyList<IQslNode> Arguments { get; }

        public QslCallNode(
            IQslNode target,
            IReadOnlyList<IQslNode> arguments)
        {
            Target = target;
            Arguments = arguments;
        }
    }
}