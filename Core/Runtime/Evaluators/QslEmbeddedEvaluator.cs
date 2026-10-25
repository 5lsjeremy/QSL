using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Evaluators
{
    internal  sealed class QslEmbeddedEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";
        public override string Name => "embedded";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslEmbeddedNode embedded)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Embedded evaluator received an incompatible node."
                );

                return null;
            }

            return context.Evaluate(
                embedded.InnerExpression
            );
        }
    }
}