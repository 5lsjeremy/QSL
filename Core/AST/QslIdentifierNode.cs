using QSL.Core.Ast.Types;

namespace QSL.Core.Ast
{
    public sealed class QslIdentifierNode : QslNode
    {
        public override QslNodeKind Kind => QslNodeKind.Identifier;

        public string Name { get; init; }

        public QslIdentifierNode(string name)
        {
            Name = name;
        }
    }
}
