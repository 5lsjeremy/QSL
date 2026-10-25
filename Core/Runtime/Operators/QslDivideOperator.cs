using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Operators
{
    internal sealed class QslDivideOperator :
        QslOperatorBase
    {
        public override string Name =>
            "/";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (!QslNumeric.TryConvertPair(
                    left,
                    right,
                    out var leftNumber,
                    out var rightNumber))
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Binary '/' requires numeric operands."
                );

                return null;
            }

            if (rightNumber == 0.0)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Division by zero is not allowed."
                );

                return null;
            }

            return leftNumber /
                   rightNumber;
        }
    }
}
