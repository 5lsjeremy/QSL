using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslTernaryEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";
        public override string Name => "ternary";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslTernaryNode ternary)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Ternary evaluator received an incompatible node."
                );

                return null;
            }

            var conditionValue =
                context.Evaluate(ternary.Condition);

            if (conditionValue is not bool condition)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Ternary condition must evaluate to a boolean."
                );

                return null;
            }

            if (condition)
            {
                return context.Evaluate(
                    ternary.TrueExpr
                );
            }

            return context.Evaluate(
                ternary.FalseExpr
            );
        }
    }
}