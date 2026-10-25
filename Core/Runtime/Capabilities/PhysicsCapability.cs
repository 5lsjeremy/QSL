using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Custom;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Functions;

namespace QSL.Core.Runtime.Capabilities
{
    public sealed class PhysicsCapability : QslRegistryItem, IQslCapability
    {
        public override string Name => "PhysicsCapability";
        public override string Type => "capability";

        public void Apply(QslRuntimeContext context)
        {
            context.Registry.RegisterType(
                new ApplyForceFunction(this)
            );

            context.Registry.RegisterType(
                new DampenVelocityFunction()
            );
        }

        public object? ApplyForce(
            object?[] args,
            QslRuntimeContext context)
        {
            if (args.Length == 2 &&
                args[0] is string entity &&
                args[1] is Vector force)
            {
                var velocity = new Velocity(
                    force.X * 0.1,
                    force.Y * 0.1
                );

                context.SetVariable(
                    $"{entity}.Velocity",
                    velocity
                );

                context.AppendSlug(
                    "force-applied"
                );

                AddProvenance(
                    QslProvenanceType.Info,
                    QslProvenanceCategory.Capability,
                    Name,
                    $"Applied force {force} to {entity}"
                );

                return velocity;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Capability,
                Name,
                "ApplyForce requires (entity, Vector) arguments."
            );

            return null;
        }
    }
}