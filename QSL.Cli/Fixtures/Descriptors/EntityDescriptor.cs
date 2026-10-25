using System;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Context;

namespace QSL.Cli.Fixtures.Descriptors
{
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
}