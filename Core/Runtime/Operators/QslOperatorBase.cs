using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Operators
{
    internal abstract class QslOperatorBase :
        QslRegistryItem,
        IQslOperator
    {
        public override string Type => "operator";

        public virtual object? ApplyUnary(
            object? operand,
            QslRuntimeContext context)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                $"Operator '{Name}' does not support unary application."
            );

            return null;
        }

        public virtual object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                $"Operator '{Name}' does not support binary application."
            );

            return null;
        }
    }
}
