using QSL.Abstractions.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Abstractions.Runtime
{
    public interface IQslDescriptor : IQslRegistryItem
    {
        void Apply(QslRuntimeContext context);
    }
}