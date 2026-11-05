using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Lexer;
using QSL.Core.Parser;
using QSL.Core.Provenance;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;

namespace QSL.Core
{
    public sealed class QslEngine
    {
        private readonly QslRuntimeContext _context;

        public QslEngine(
            QslRuntimeContext context)
        {
            _context =
                context
                ?? throw new ArgumentNullException(
                    nameof(context)
                );
        }

        /// <summary>
        /// Makes an exact consumer-owned runtime object available
        /// through its supplied QSL identifier.
        /// </summary>
        public void BindConsumer(
            QslConsumerBinding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException(
                    nameof(binding)
                );
            }

            _context.SetVariable(
                binding.Name,
                binding.Value
            );
        }

        /// <summary>
        /// Removes a consumer binding only when the identifier
        /// still references the exact object supplied by the
        /// binding.
        ///
        /// Successfully unbinding a consumer also retires all
        /// attached state associated with that exact object.
        /// </summary>
        public bool UnbindConsumer(
            QslConsumerBinding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException(
                    nameof(binding)
                );
            }

            var unbound =
                _context.RemoveVariable(
                    binding.Name,
                    binding.Value
                );

            if (!unbound)
            {
                return false;
            }

            _context.RemoveState(
                binding.Value
            );

            return true;
        }

        /// <summary>
        /// Receives one approved state instruction for the exact
        /// target object.
        /// </summary>
        public bool ReceiveState(
            object target,
            string propertyName,
            object? value)
        {
            return _context.ReceiveState(
                target,
                propertyName,
                value
            );
        }

        /// <summary>
        /// Receives a package of approved state instructions for
        /// one exact target object.
        ///
        /// Every package property retains the same non-destructive
        /// receipt semantics as the single-state overload.
        ///
        /// The return value reports how many properties were newly
        /// received.
        /// </summary>
        public int ReceiveState(
            QslStatePackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package)
                );
            }

            var received =
                0;

            foreach (var property in
                     package.Properties)
            {
                if (ReceiveState(
                        package.Target,
                        property.Key,
                        property.Value))
                {
                    received++;
                }
            }

            package.RecordApplied(
                received
            );

            return received;
        }

        /// <summary>
        /// Applies an approved update to existing attached state.
        ///
        /// This operation does not introduce missing state.
        /// </summary>
        public bool UpdateState(
            object target,
            string propertyName,
            object? value)
        {
            return _context.UpdateState(
                target,
                propertyName,
                value
            );
        }

        /// <summary>
        /// Applies resolved state values that Wilds has already
        /// calculated through its decay processing.
        ///
        /// Nonzero values update existing attached state.
        /// Numeric zero removes existing state when RemoveAtZero
        /// is enabled by the package.
        ///
        /// Decay cannot introduce missing state.
        /// </summary>
        public int ReceiveDecay(
            QslDecayPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package)
                );
            }

            var affected =
                0;

            foreach (var property in
                     package.Properties)
            {
                if (package.RemoveAtZero &&
                    IsZero(
                        property.Value))
                {
                    if (RemoveState(
                            package.Target,
                            property.Key))
                    {
                        affected++;
                    }

                    continue;
                }

                if (UpdateState(
                        package.Target,
                        property.Key,
                        property.Value))
                {
                    affected++;
                }
            }

            package.RecordApplied(
                affected
            );

            return affected;
        }

        /// <summary>
        /// Removes one attached-state property from the exact
        /// target object.
        /// </summary>
        public bool RemoveState(
            object target,
            string propertyName)
        {
            return _context.RemoveState(
                target,
                propertyName
            );
        }

        /// <summary>
        /// Removes all attached state associated with the exact
        /// target object.
        /// </summary>
        public bool RemoveState(
            object target)
        {
            return _context.RemoveState(
                target
            );
        }

        /// <summary>
        /// Clears active runtime variables, metadata, and attached
        /// state while preserving the configured runtime registry.
        /// </summary>
        public void Clear()
        {
            _context.Clear();
        }

        public QslParseResult Parse(
            string source)
        {
            var tokenizer =
                new QslTokenizer(
                    source
                );

            var tokens =
                tokenizer.Tokenize();

            var parser =
                new QslParser(
                    tokens
                );

            var ast =
                parser.ParseExpression();

            return new QslParseResult(
                ast,
                CopyDiagnostics(
                    parser.Errors
                )
            );
        }

        public QslProcessingResult Evaluate(
            string source)
        {
            var provenanceStart =
                QslRuntime.Instance
                    .ProvenanceBus
                    .GetAll()
                    .Count;

            try
            {
                var parseResult =
                    Parse(
                        source
                    );

                if (parseResult.HasDiagnostics)
                {
                    return QslProcessingResult.ParseFailure(
                        parseResult.Ast,
                        parseResult.Diagnostics
                    );
                }

                var value =
                    _context.Evaluate(
                        parseResult.Ast
                    );

                var runtimeDiagnostics =
                    GetRuntimeDiagnosticsSince(
                        provenanceStart
                    );

                if (runtimeDiagnostics.Count > 0)
                {
                    return QslProcessingResult.EvaluationFailure(
                        parseResult.Ast,
                        value,
                        runtimeDiagnostics
                    );
                }

                return QslProcessingResult.Success(
                    parseResult.Ast,
                    value
                );
            }
            catch (Exception exception)
            {
                var runtimeDiagnostics =
                    GetRuntimeDiagnosticsSince(
                        provenanceStart
                    );

                return QslProcessingResult.ExceptionFailure(
                    exception,
                    runtimeDiagnostics
                );
            }
        }

        private static bool IsZero(
            object? value)
        {
            switch (value)
            {
                case byte number:
                    return number == 0;

                case sbyte number:
                    return number == 0;

                case short number:
                    return number == 0;

                case ushort number:
                    return number == 0;

                case int number:
                    return number == 0;

                case uint number:
                    return number == 0;

                case long number:
                    return number == 0;

                case ulong number:
                    return number == 0;

                case float number:
                    return number == 0.0f;

                case double number:
                    return number == 0.0;

                case decimal number:
                    return number == 0.0m;

                default:
                    return false;
            }
        }

        private static IReadOnlyList<string>
            CopyDiagnostics(
                IReadOnlyList<string> diagnostics)
        {
            if (diagnostics == null ||
                diagnostics.Count == 0)
            {
                return Array.Empty<string>();
            }

            var copy =
                new string[diagnostics.Count];

            for (
                var index = 0;
                index < diagnostics.Count;
                index++)
            {
                copy[index] =
                    diagnostics[index];
            }

            return copy;
        }

        private static IReadOnlyList<QslProvenanceEntry>
            GetRuntimeDiagnosticsSince(
                int startIndex)
        {
            var allEntries =
                QslRuntime.Instance
                    .ProvenanceBus
                    .GetAll();

            if (startIndex < 0)
            {
                startIndex =
                    0;
            }

            if (startIndex >= allEntries.Count)
            {
                return Array.Empty<QslProvenanceEntry>();
            }

            var diagnostics =
                new List<QslProvenanceEntry>();

            for (
                var index = startIndex;
                index < allEntries.Count;
                index++)
            {
                var entry =
                    allEntries[index];

                if (entry.EntryType ==
                    QslProvenanceType.Error)
                {
                    diagnostics.Add(
                        entry
                    );
                }
            }

            return diagnostics.Count == 0
                ? Array.Empty<QslProvenanceEntry>()
                : diagnostics.ToArray();
        }
    }

    public sealed class QslParseResult
    {
        public IQslNode Ast { get; }

        public IReadOnlyList<string>
            Diagnostics { get; }

        public bool HasDiagnostics =>
            Diagnostics.Count > 0;

        public QslParseResult(
            IQslNode ast,
            IReadOnlyList<string> diagnostics)
        {
            Ast =
                ast
                ?? throw new ArgumentNullException(
                    nameof(ast)
                );

            Diagnostics =
                diagnostics
                ?? Array.Empty<string>();
        }
    }

    public sealed class QslProcessingResult
    {
        public IQslNode? Ast { get; }

        public object? Value { get; }

        public IReadOnlyList<string>
            ParserDiagnostics { get; }

        public IReadOnlyList<QslProvenanceEntry>
            RuntimeDiagnostics { get; }

        public Exception? Exception { get; }

        public bool ParseFailed =>
            ParserDiagnostics.Count > 0;

        public bool EvaluationFailed =>
            RuntimeDiagnostics.Count > 0;

        public bool HasException =>
            Exception is not null;

        public bool Succeeded =>
            !ParseFailed &&
            !EvaluationFailed &&
            !HasException;

        private QslProcessingResult(
            IQslNode? ast,
            object? value,
            IReadOnlyList<string> parserDiagnostics,
            IReadOnlyList<QslProvenanceEntry>
                runtimeDiagnostics,
            Exception? exception)
        {
            Ast =
                ast;

            Value =
                value;

            ParserDiagnostics =
                parserDiagnostics
                ?? Array.Empty<string>();

            RuntimeDiagnostics =
                runtimeDiagnostics
                ?? Array.Empty<QslProvenanceEntry>();

            Exception =
                exception;
        }

        public static QslProcessingResult Success(
            IQslNode ast,
            object? value)
        {
            return new QslProcessingResult(
                ast,
                value,
                Array.Empty<string>(),
                Array.Empty<QslProvenanceEntry>(),
                null
            );
        }

        public static QslProcessingResult ParseFailure(
            IQslNode ast,
            IReadOnlyList<string> diagnostics)
        {
            return new QslProcessingResult(
                ast,
                null,
                diagnostics,
                Array.Empty<QslProvenanceEntry>(),
                null
            );
        }

        public static QslProcessingResult EvaluationFailure(
            IQslNode ast,
            object? value,
            IReadOnlyList<QslProvenanceEntry>
                runtimeDiagnostics)
        {
            return new QslProcessingResult(
                ast,
                value,
                Array.Empty<string>(),
                runtimeDiagnostics,
                null
            );
        }

        public static QslProcessingResult ExceptionFailure(
            Exception exception,
            IReadOnlyList<QslProvenanceEntry>
                runtimeDiagnostics)
        {
            return new QslProcessingResult(
                null,
                null,
                Array.Empty<string>(),
                runtimeDiagnostics,
                exception
            );
        }
    }
}