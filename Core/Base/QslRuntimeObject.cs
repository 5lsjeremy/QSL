using QSL.Abstractions.Base;

namespace QSL.Core.Base
{
    public abstract class QslRuntimeObject : QslBaseObject, IQslRuntimeObject
    {
        public abstract string Name { get; }
    }
}
