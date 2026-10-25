using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Descriptors
{
    public sealed class NPCDescriptor : QslRegistryItem, IQslDescriptor
    {
        public override string Name => "NPC";
        public override string Type => "descriptor";

        public void Apply(QslRuntimeContext context)
        {
            context.SetVariable("npc.health", 50);
            context.SetVariable("npc.aggression", 0.25);
        }
    }
}