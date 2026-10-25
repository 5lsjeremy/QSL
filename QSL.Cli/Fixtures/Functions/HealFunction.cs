using System;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

namespace QSL.Cli.Fixtures.Functions
{
    public sealed class HealFunction :
        QslRegistryItem,
        IQslFunction
    {
        public override string Type =>
            "function";

        public override string Name =>
            "heal";

        public object? Invoke(
            object?[] args,
            QslRuntimeContext context)
        {
            if (args.Length != 1)
            {
                ReportInvalidArguments();
                return null;
            }

            if (!TryGetAmount(
                    args[0],
                    out var amount))
            {
                ReportInvalidArguments();
                return null;
            }

            var resolved =
                context.ResolveIdentifier(
                    "health"
                );

            int current;

            try
            {
                current =
                    resolved is int integer
                        ? integer
                        : Convert.ToInt32(
                            resolved ?? 0
                        );
            }
            catch (
                FormatException)
            {
                ReportInvalidHealth(
                    resolved
                );

                return null;
            }
            catch (
                InvalidCastException)
            {
                ReportInvalidHealth(
                    resolved
                );

                return null;
            }
            catch (
                OverflowException)
            {
                ReportInvalidHealth(
                    resolved
                );

                return null;
            }

            var newHealth =
                current + amount;

            context.SetVariable(
                "health",
                newHealth
            );

            return newHealth;
        }

        private static bool TryGetAmount(
            object? value,
            out int amount)
        {
            switch (value)
            {
                case int integer:
                    amount = integer;
                    return true;

                case double number
                    when !double.IsNaN(number) &&
                         !double.IsInfinity(number) &&
                         number >= int.MinValue &&
                         number <= int.MaxValue:
                    amount =
                        Convert.ToInt32(
                            number
                        );

                    return true;

                default:
                    amount = 0;
                    return false;
            }
        }

        private void ReportInvalidArguments()
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "HealFunction requires exactly one numeric argument."
            );
        }

        private void ReportInvalidHealth(
            object? value)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "HealFunction could not convert the current " +
                $"health value '{value ?? "null"}' to an integer."
            );
        }
    }
}
