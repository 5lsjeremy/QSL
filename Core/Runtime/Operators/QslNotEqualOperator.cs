using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslNotEqualOperator :
        QslOperatorBase
    {
        public override string Name =>
            "!=";

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
                return leftNumber !=
                       rightNumber;
            }

            return !Equals(
                left,
                right
            );
        }
    }
}