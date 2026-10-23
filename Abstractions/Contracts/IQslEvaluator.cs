using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;

namespace QSL.Abstractions.Base
{
    public interface IQslEvaluator : IQslRegistryItem
    {
        object? Evaluate(IQslNode node, QslRuntimeContext context);
    }    
}
