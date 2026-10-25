using System;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Operators
{
    internal sealed class QslGreaterEqualOperator :
        QslOperatorBase
    {
        public override string Name =>
            ">=";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (TryCompare(
                    left,
                    right,
                    out var comparison))
            {
                return comparison >= 0;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '>=' requires comparable operands."
            );

            return null;
        }

        private static bool TryCompare(
            object? left,
            object? right,
            out int comparison)
        {
            comparison = 0;

            if (QslNumeric.TryConvertPair(
                    left,
                    right,
                    out var leftNumber,
                    out var rightNumber))
            {
                comparison =
                    leftNumber.CompareTo(
                        rightNumber
                    );

                return true;
            }

            if (left is null ||
                right is null)
            {
                return false;
            }

            if (left.GetType() !=
                right.GetType())
            {
                return false;
            }

            if (left is not
                IComparable comparable)
            {
                return false;
            }

            comparison =
                comparable.CompareTo(
                    right
                );

            return true;
        }
    }
}
