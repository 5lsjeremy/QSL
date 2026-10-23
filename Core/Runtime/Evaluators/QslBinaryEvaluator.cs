using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Lexer;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Operators;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslBinaryEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "binary";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslBinaryNode binary)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Binary evaluator received an incompatible node."
                );

                return null;
            }

            var strategy =
                QslKindExtensions.EvaluationStrategy(
                    binary.OperatorKind);

            switch (strategy)
            {
                case QslEvaluationStrategy.Eager:
                    return EvaluateEager(
                        binary,
                        context);

                case QslEvaluationStrategy.ShortCircuitAnd:
                    return EvaluateShortCircuitAnd(
                        binary,
                        context);

                case QslEvaluationStrategy.ShortCircuitOr:
                    return EvaluateShortCircuitOr(
                        binary,
                        context);

                case QslEvaluationStrategy.NullCoalescing:
                    return EvaluateNullCoalescing(
                        binary,
                        context);

                case QslEvaluationStrategy.Arrow:
                    return EvaluateArrow(
                        binary,
                        context);
                
                case QslEvaluationStrategy.Assignment:
                    return EvaluateAssignment(
                        binary,
                        context);
                
                default:
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        Name,
                        $"Unsupported evaluation strategy " +
                        $"'{strategy}' for " +
                        $"'{binary.OperatorKind}'."
                    );

                    return null;
            }
        }

        private object? EvaluateEager(
            QslBinaryNode binary,
            QslRuntimeContext context)
        {
            var leftValue =
                context.Evaluate(binary.Left);

            var rightValue =
                context.Evaluate(binary.Right);

            var executor =
                QslOperatorResolver.Resolve(
                    binary.OperatorKind);

            return executor.ApplyBinary(
                leftValue,
                rightValue,
                context);
        }

        private object? EvaluateShortCircuitAnd(
            QslBinaryNode binary,
            QslRuntimeContext context)
        {
            var leftValue =
                context.Evaluate(binary.Left);

            if (leftValue is not bool leftBoolean)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Left operand of '&&' must be boolean."
                );

                return null;
            }

            if (!leftBoolean)
                return false;

            var rightValue =
                context.Evaluate(binary.Right);

            if (rightValue is not bool rightBoolean)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Right operand of '&&' must be boolean."
                );

                return null;
            }

            var executor =
                QslOperatorResolver.Resolve(
                    binary.OperatorKind);

            return executor.ApplyBinary(
                leftBoolean,
                rightBoolean,
                context);
        }

        private object? EvaluateShortCircuitOr(
            QslBinaryNode binary,
            QslRuntimeContext context)
        {
            var leftValue =
                context.Evaluate(binary.Left);

            if (leftValue is not bool leftBoolean)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Left operand of '||' must be boolean."
                );

                return null;
            }

            if (leftBoolean)
                return true;

            var rightValue =
                context.Evaluate(binary.Right);

            if (rightValue is not bool rightBoolean)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Right operand of '||' must be boolean."
                );

                return null;
            }

            var executor =
                QslOperatorResolver.Resolve(
                    binary.OperatorKind);

            return executor.ApplyBinary(
                leftBoolean,
                rightBoolean,
                context);
        }

        private object? EvaluateNullCoalescing(
            QslBinaryNode binary,
            QslRuntimeContext context)
        {
            var leftValue =
                context.Evaluate(binary.Left);

            if (leftValue is not null)
                return leftValue;

            var rightValue =
                context.Evaluate(binary.Right);

            var executor =
                QslOperatorResolver.Resolve(
                    binary.OperatorKind);

            return executor.ApplyBinary(
                leftValue,
                rightValue,
                context);
        }

private object? EvaluateAssignment(
    QslBinaryNode binary,
    QslRuntimeContext context)
{
    if (binary.Left is
        QslIdentifierNode identifier)
    {
        var value =
            context.Evaluate(
                binary.Right
            );

        context.SetVariable(
            identifier.Name,
            value
        );

        return value;
    }

    if (binary.Left is
        QslMemberAccessNode memberAccess)
    {
        var targetValue =
            context.Evaluate(
                memberAccess.Target
            );

        if (targetValue is null)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Assignment member target evaluated to null."
            );

            return null;
        }

        if (targetValue is
            IQslDescriptor)
        {
            var value =
                context.Evaluate(
                    binary.Right
                );

            context.SetVariable(
                memberAccess.MemberName,
                value
            );

            return value;
        }

        var targetType =
            targetValue.GetType();

        var property =
            targetType.GetProperty(
                memberAccess.MemberName
            );

        if (property is not null &&
            property.CanWrite)
        {
            var value =
                context.Evaluate(
                    binary.Right
                );

            property.SetValue(
                targetValue,
                value,
                null
            );

            return value;
        }

        var field =
            targetType.GetField(
                memberAccess.MemberName
            );

        if (field is not null)
        {
            var value =
                context.Evaluate(
                    binary.Right
                );

            field.SetValue(
                targetValue,
                value
            );

            return value;
        }

        AddProvenance(
            QslProvenanceType.Error,
            QslProvenanceCategory.Evaluation,
            Name,
            $"Member '{memberAccess.MemberName}' " +
            "is not assignable."
        );

        return null;
    }

    AddProvenance(
        QslProvenanceType.Error,
        QslProvenanceCategory.Evaluation,
        Name,
        "Left operand of assignment must be " +
        "an identifier or assignable member."
    );

    return null;
}
        
        private object? EvaluateArrow(
            QslBinaryNode binary,
            QslRuntimeContext context)
        {
            context.Evaluate(binary.Left);

            return context.Evaluate(binary.Right);
        }
    }
}