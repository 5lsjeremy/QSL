using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslErrorEvaluator : QslRegistryItem, IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => nameof(QslErrorEvaluator);

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslErrorNode error)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    $"Expected {nameof(QslErrorNode)} but received {node.GetType().Name}."
                );

                return null;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                error.Message
            );

            return null;
        }
    }
}