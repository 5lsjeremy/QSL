using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslInterpolatedStringNode :
        QslNode
    {
        public override QslNodeKind Kind =>
            QslNodeKind.InterpolatedString;

        public IReadOnlyList<IQslNode> Parts { get; }

        public QslInterpolatedStringNode(
            IReadOnlyList<IQslNode> parts)
        {
            Parts = parts;
        }
    }
}