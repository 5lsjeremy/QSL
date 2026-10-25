using QSL.Abstractions.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Abstractions.Runtime
{
    public interface IQslFunction : IQslRegistryItem
    {
        object? Invoke(object?[] args, QslRuntimeContext context);
    }
}