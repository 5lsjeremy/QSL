using QSL.Abstractions.Base;
using QSL.Core.Ast;
using QSL.Core.Ast.Types;

namespace QSL.Abstractions.Runtime
{
    public interface IQslNode : IQslObject
    {
        QslNodeKind Kind { get; }
    }
}