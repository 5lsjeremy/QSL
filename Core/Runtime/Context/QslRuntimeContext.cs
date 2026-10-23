using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime;

public sealed class QslRuntimeContext : QslContextBase
{
    public QslRuntimeRegistry Registry { get; }

    public QslRuntimeContext(QslRuntimeRegistry registry)
    {
        Registry = registry;
    }

    public override string Type => "runtime-context";

    public object? Evaluate(IQslNode node)
    {
        var key = node.Kind;

        if (Registry.Evaluators.TryGetValue(key, out var evaluator))
            return evaluator.Evaluate(node, this);

        AddProvenance(
            QslProvenanceType.Error,
            QslProvenanceCategory.Evaluation,
            "QslRuntimeContext",
            $"No evaluator registered for node kind: {key}"
        );

        return null;
    }

    public object? ResolveIdentifier(string idName)
    {
        if (string.IsNullOrEmpty(idName))
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                "QslRuntimeContext",
                "Attempted to resolve a null/empty identifier."
            );

            return null;
        }

        var variable = GetVariable(idName);

        if (variable != null)
            return variable;

        if (Registry.Functions.TryGetValue(idName, out var function))
            return function;

        if (Registry.Descriptors.TryGetValue(idName, out var descriptor))
            return descriptor;

        if (Registry.Capabilities.TryGetValue(idName, out var capability))
            return capability;

        AddProvenance(
            QslProvenanceType.Error,
            QslProvenanceCategory.Evaluation,
            "QslRuntimeContext",
            $"Unresolved identifier: {idName}"
        );

        return null;
    }
}
