using QSL.Abstractions.Base;

namespace QSL.Abstractions.Runtime
{
    public interface IQslDescriptor : IQslRegistryItem
    {
        void Apply(QslRuntimeContext context);
    }
}