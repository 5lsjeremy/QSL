using QSL.Abstractions.Base;

namespace QSL.Abstractions.Runtime
{
    public interface IQslBlockFactory : IQslRegistryItem
    {
        IQslBlock Create();
    }
}