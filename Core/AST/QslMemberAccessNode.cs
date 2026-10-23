using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslMemberAccessNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.MemberAccess;

        public IQslNode Target { get; init; }
        public string MemberName { get; init; }

        public QslMemberAccessNode(IQslNode target, string memberName)
        {
            Target = target;
            MemberName = memberName;
        }
    }    
}
