using QSL.Abstractions.Runtime;
using QSL.Core.Ast.Types;
using QSL.Core.Base;

namespace QSL.Core.Ast
{
    public abstract class QslNode : QslBaseObject, IQslNode
    {
        public abstract QslNodeKind Kind { get; }
        public override string Type => "node";
    }
}


