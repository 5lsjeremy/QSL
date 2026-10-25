using System.Linq;
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslCallEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "call";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslCallNode call)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Call evaluator received an incompatible node."
                );

                return null;
            }

            var targetObject =
                context.Evaluate(call.Target);

            if (targetObject is not IQslFunction function)
            {
                var targetName =
                    call.Target is QslIdentifierNode identifier
                        ? identifier.Name
                        : call.Target.ToString() ?? "<unknown>";

                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    $"Identifier '{targetName}' is not callable."
                );

                return null;
            }

            var arguments = call.Arguments
                .Select(argument =>
                    context.Evaluate(argument))
                .ToArray();

            return function.Invoke(
                arguments,
                context);
        }
    }
}