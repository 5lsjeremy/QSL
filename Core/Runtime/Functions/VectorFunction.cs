using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Custom;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Helpers;

namespace QSL.Core.Runtime.Functions
{
    public sealed class VectorFunction :
        QslRegistryItem,
        IQslFunction
    {
        public override string Type =>
            "function";

        public override string Name =>
            "Vector";

        public object? Invoke(
            object?[] args,
            QslRuntimeContext context)
        {
            if (args.Length != 2 &&
                args.Length != 3)
            {
                ReportInvalidArguments();
                return null;
            }

            if (!QslNumeric.TryConvert(
                    args[0],
                    out var x))
            {
                ReportInvalidArguments();
                return null;
            }

            if (!QslNumeric.TryConvert(
                    args[1],
                    out var y))
            {
                ReportInvalidArguments();
                return null;
            }

            if (args.Length == 2)
            {
                return new Vector(
                    x,
                    y
                );
            }

            if (!QslNumeric.TryConvert(
                    args[2],
                    out var z))
            {
                ReportInvalidArguments();
                return null;
            }

            return new Vector(
                x,
                y,
                z
            );
        }

        private void ReportInvalidArguments()
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Vector requires two or three numeric arguments."
            );
        }
    }
}