using QSL.Abstractions.Base;

namespace QSL.Abstractions.Runtime
{
    public interface IQslFunction : IQslRegistryItem
    {
        object? Invoke(object?[] args, QslRuntimeContext context);
    }
}