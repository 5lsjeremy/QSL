using QSL.Abstractions.Base;

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