using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslNotOperator : QslOperatorBase
    {
        public override string Type => "operator";
        public override string Name => "!";

        public override object? ApplyUnary(
            object? operand,
            QslRuntimeContext context)
        {
            if (operand is bool value)
                return !value;

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Unary '!' requires a boolean operand."
            );

            return null;
        }
    }
}