using QSL.Core.Provenance;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslMultiplyOperator :
        QslOperatorBase
    {
        public override string Name =>
            "*";

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
                return leftNumber *
                       rightNumber;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '*' requires numeric operands."
            );

            return null;
        }
    }
}