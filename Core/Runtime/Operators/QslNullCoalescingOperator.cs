using QSL.Abstractions.Runtime;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslNullCoalescingOperator : QslOperatorBase
    {
        public override string Type => "operator";
        public override string Name => "??";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            return left ?? right;
        }
    }
}