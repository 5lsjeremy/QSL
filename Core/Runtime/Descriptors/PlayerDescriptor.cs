using System;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;

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

public sealed class EntityDescriptor : QslRegistryItem, IQslDescriptor
{
    public override string Name => "Entity";
    public override string Type => "descriptor";

    public void Apply(QslRuntimeContext context)
    {
        context.SetVariable("entity.id", Guid.NewGuid().ToString());
        context.SetVariable("entity.active", true);
    }
}

