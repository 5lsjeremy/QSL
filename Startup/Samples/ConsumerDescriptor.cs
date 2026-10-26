using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Context;

namespace QSL.ConsumerTest
{
    public sealed class CustomerDescriptor :
        QslRegistryItem,
        IQslDescriptor
    {
        public override string Name => "Customer";

        public override string Type => "descriptor";

        public void Apply(QslRuntimeContext context)
        {
            context.SetVariable(
                "health",
                100
            );

            context.SetVariable(
                "mana",
                50
            );

            context.SetVariable(
                "isAlive",
                true
            );
        }
    }
}