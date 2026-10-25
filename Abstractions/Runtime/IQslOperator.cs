using QSL.Abstractions.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Abstractions.Runtime
{
    public interface IQslOperator : IQslRegistryItem
    {
        object? ApplyUnary(
            object? operand,
            QslRuntimeContext context);

        object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context);
    }
}