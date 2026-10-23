using QSL.Abstractions.Base;

namespace QSL.Abstractions.Runtime
{
    public interface IQslCapability : IQslRegistryItem
    {
        void Apply(QslRuntimeContext context);
    }
}