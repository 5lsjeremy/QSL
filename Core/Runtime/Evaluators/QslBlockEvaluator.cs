using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslBlockEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "Block";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslBlockNode block)
            {
                const string message =
                    "QslBlockEvaluator received a non-block node.";

                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    message
                );

                return new QslErrorNode(message);
            }

            object? lastValue = null;

            foreach (var statement in block.Statements)
            {
                if (statement is QslErrorNode error)
                {
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        Name,
                        error.Message
                    );

                    lastValue = error;
                    continue;
                }

                lastValue =
                    context.Evaluate(statement);
            }

            return lastValue;
        }
    }
}