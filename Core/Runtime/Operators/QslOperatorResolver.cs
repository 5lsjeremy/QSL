using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Lexer;

namespace QSL.Core.Runtime.Operators
{
    public static class QslOperatorResolver
    {
        private static readonly Dictionary<
            QslTokenKind,
            IQslOperator> Instances =
                new Dictionary<QslTokenKind, IQslOperator>();

        public static IQslOperator Resolve(
            QslTokenKind operatorKind)
        {
            IQslOperator existing;

            if (Instances.TryGetValue(
                    operatorKind,
                    out existing))
            {
                return existing;
            }

            var executorType =
                QslKindExtensions.ExecutorType(
                    operatorKind);

            if (!typeof(IQslOperator).IsAssignableFrom(
                    executorType))
            {
                throw new InvalidOperationException(
                    $"Executor type '{executorType.FullName}' " +
                    $"for QslTokenKind.{operatorKind} does not " +
                    $"implement {nameof(IQslOperator)}.");
            }

            var constructor =
                executorType.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"Executor type '{executorType.FullName}' " +
                    $"for QslTokenKind.{operatorKind} must have " +
                    "a public parameterless constructor.");
            }

            var instance =
                Activator.CreateInstance(executorType)
                as IQslOperator;

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create executor " +
                    $"'{executorType.FullName}' for " +
                    $"QslTokenKind.{operatorKind}.");
            }

            Instances.Add(
                operatorKind,
                instance);

            return instance;
        }

        public static bool IsResolved(
            QslTokenKind operatorKind)
        {
            return Instances.ContainsKey(
                operatorKind);
        }

        public static void Clear()
        {
            Instances.Clear();
        }
    }
}