using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslSubtractOperator :
        QslOperatorBase
    {
        public override string Name =>
            "-";

        public override object? ApplyUnary(
            object? operand,
            QslRuntimeContext context)
        {
            if (QslNumeric.TryConvert(
                    operand,
                    out var number))
            {
                return -number;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Unary '-' requires a numeric operand."
            );

            return null;
        }

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (QslNumeric.TryConvertPair(
                    left,
                    right,
                    out var leftNumber,
                    out var rightNumber))
            {
                return leftNumber -
                       rightNumber;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '-' requires numeric operands."
            );

            return null;
        }
    }
}