using QSL.Abstractions.Base;

namespace QSL.Abstractions.Runtime
{
    public interface IQslBlock : IQslObject
    {
        void Execute(QslRuntimeContext context);
    }
}