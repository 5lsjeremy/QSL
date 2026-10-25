using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Operators;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslUnaryEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";
        public override string Name => "unary";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslUnaryNode unary)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Unary evaluator received an incompatible node."
                );

                return null;
            }

            var operandValue =
                context.Evaluate(unary.Operand);

            var executor =
                QslOperatorResolver.Resolve(
                    unary.OperatorKind);

            return executor.ApplyUnary(
                operandValue,
                context
            );
        }
    }
}