using QSL.Core.Provenance;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslAndOperator : QslOperatorBase
    {
        public override string Type => "operator";
        public override string Name => "&&";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (left is bool leftBoolean &&
                right is bool rightBoolean)
            {
                return leftBoolean && rightBoolean;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '&&' requires boolean operands."
            );

            return null;
        }
    }
}