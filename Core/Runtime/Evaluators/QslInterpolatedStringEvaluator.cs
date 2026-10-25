using System;
using System.Globalization;
using System.Text;
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Core.Runtime.Evaluators
{
    internal  sealed class QslInterpolatedStringEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type =>
            "evaluator";

        public override string Name =>
            "interpolated-string";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not
                QslInterpolatedStringNode interpolated)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Interpolated-string evaluator received " +
                    "an incompatible node."
                );

                return null;
            }

            var result =
                new StringBuilder();

            foreach (var part in
                     interpolated.Parts)
            {
                var value =
                    context.Evaluate(
                        part
                    );

                result.Append(
                    FormatValue(
                        value
                    )
                );
            }

            return result.ToString();
        }

        private static string FormatValue(
            object? value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (value is bool boolean)
            {
                return boolean
                    ? "true"
                    : "false";
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture
                );
            }

            return value.ToString() ??
                   string.Empty;
        }
    }
}