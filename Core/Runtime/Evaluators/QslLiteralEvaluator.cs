using System;
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Ast.Types;
using QSL.Core.Base;
using QSL.Core.Custom;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslLiteralEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "literal";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslLiteralNode literal)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Literal evaluator received an incompatible node."
                );

                return null;
            }

            return literal.LiteralKind switch
            {
                QslLiteralKind.Number =>
                    Convert.ToDouble(literal.Value),

                QslLiteralKind.String =>
                    literal.Value?.ToString(),

                QslLiteralKind.Boolean =>
                    Convert.ToBoolean(literal.Value),

                QslLiteralKind.Null =>
                    null,

                QslLiteralKind.Vector =>
                    (Vector)literal.Value!,

                QslLiteralKind.Velocity =>
                    (Velocity)literal.Value!,

                _ =>
                    literal.Value
            };
        }
    }
}