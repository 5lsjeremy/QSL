using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslBlockNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Block;

        public IReadOnlyList<IQslNode> Statements { get; }

        public QslBlockNode(IReadOnlyList<IQslNode> statements)
        {
            Statements = statements;
        }
    }
}