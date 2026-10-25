using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Cli.Fixtures.Descriptors
{
    public sealed class PlayerDescriptor : QslRegistryItem, IQslDescriptor
    {
        public override string Name => "Player";
        public override string Type => "descriptor";

        public void Apply(QslRuntimeContext context)
        {
            context.SetVariable("health", 100);
            context.SetVariable("mana", 50);
        }
    }
}
