using System;
using System.Collections.Generic;
using System.Reflection;

namespace QSL.Core.Lexer
{
    public static class QslKindExtensions
    {
        private static readonly IReadOnlyDictionary<
            QslTokenKind,
            QslTokenAttribute> Metadata = BuildMetadata();

        public static QslTokenAttribute Info(
            this QslTokenKind kind)
        {
            if (Metadata.TryGetValue(kind, out var info))
                return info;

            throw new InvalidOperationException(
                $"No metadata is registered for {kind}.");
        }

        public static int Precedence(
            this QslTokenKind kind)
            => kind.Info().Precedence;

        public static int UnaryPrecedence(
            this QslTokenKind kind)
            => kind.Info().UnaryPrecedence;

        public static bool RightAssociative(
            this QslTokenKind kind)
            => kind.Info().RightAssociative;

        public static bool IsUnary(
            this QslTokenKind kind)
            => kind.Info().Unary;

        public static bool IsBinary(
            this QslTokenKind kind)
            => kind.Info().Binary;

        public static string Symbol(
            this QslTokenKind kind)
            => kind.Info().Symbol;

        private static IReadOnlyDictionary<
            QslTokenKind,
            QslTokenAttribute> BuildMetadata()
        {
            var metadata =
                new Dictionary<QslTokenKind, QslTokenAttribute>();

            foreach (QslTokenKind kind in Enum.GetValues(typeof(QslTokenKind)))
            {
                var member = typeof(QslTokenKind)
                    .GetMember(kind.ToString());

                if (member.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Unable to reflect QSL token kind {kind}.");
                }

                var attribute =
                    member[0].GetCustomAttribute<QslTokenAttribute>();

                if (attribute is null)
                {
                    throw new InvalidOperationException(
                        $"QslTokenKind.{kind} has no " +
                        $"{nameof(QslTokenAttribute)}.");
                }

                if (attribute.Unary &&
                    attribute.UnaryPrecedence <= 0)
                {
                    throw new InvalidOperationException(
                        $"Unary token QslTokenKind.{kind} must " +
                        "define UnaryPrecedence.");
                }

                if (attribute.Binary &&
                    attribute.Precedence <= 0)
                {
                    throw new InvalidOperationException(
                        $"Binary token QslTokenKind.{kind} must " +
                        "define Precedence.");
                }

                metadata.Add(kind, attribute);
            }

            return metadata;
        }

        public static Type ExecutorType(
            this QslTokenKind kind)
        {
            return kind.Info().ExecutorType
                   ?? throw new InvalidOperationException(
                       $"QslTokenKind.{kind} has no executor.");
        }

        public static QslEvaluationStrategy EvaluationStrategy(
            this QslTokenKind kind)
            => kind.Info().EvaluationStrategy;
    }
}