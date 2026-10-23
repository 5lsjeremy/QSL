using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;

public sealed class DampenVelocityFunction :
    QslRegistryItem,
    IQslFunction
{
    public override string Type => "function";

    public override string Name => "DampenVelocity";

    public object? Invoke(
        object?[] args,
        QslRuntimeContext context)
    {
        if (args.Length == 2 &&
            args[0] is Velocity velocity &&
            args[1] is double factor)
        {
            var dampened =
                velocity.Scale(factor);

            context.SetVariable(
                "LastDampenedVelocity",
                dampened
            );

            AddProvenance(
                QslProvenanceType.Info,
                QslProvenanceCategory.Capability,
                Name,
                $"Dampened velocity {velocity} by factor {factor}"
            );

            return dampened;
        }

        AddProvenance(
            QslProvenanceType.Error,
            QslProvenanceCategory.Capability,
            Name,
            "DampenVelocity requires (Velocity, double) arguments."
        );

        return null;
    }
}