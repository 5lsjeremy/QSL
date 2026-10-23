using System;
using System.Collections.Generic;
using QSL.Abstractions.Runtime;
using QSL.Core.Lexer;
using QSL.Core.Parser;
using QSL.Core.Provenance;
using QSL.Core.Runtime;

namespace QSL.Core
{
    public sealed class QslEngine
    {
        private readonly QslRuntimeContext _context;

        public QslEngine(
            QslRuntimeContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(
                    nameof(context));
        }

        public QslParseResult Parse(
            string source)
        {
            var tokenizer =
                new QslTokenizer(source);

            var tokens =
                tokenizer.Tokenize();

            var parser =
                new QslParser(tokens);

            var ast =
                parser.ParseExpression();

            return new QslParseResult(
                ast,
                CopyDiagnostics(parser.Errors)
            );
        }

        public QslProcessingResult Evaluate(
            string source)
        {
            var provenanceStart =
                QslRuntime.Instance.ProvenanceBus
                    .GetAll()
                    .Count;

            try
            {
                var parseResult =
                    Parse(source);

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
                QslRuntime.Instance.ProvenanceBus
                    .GetAll();

            if (startIndex < 0)
                startIndex = 0;

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
                    diagnostics.Add(entry);
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
            Ast = ast
                ?? throw new ArgumentNullException(
                    nameof(ast));

            Diagnostics = diagnostics
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
            Ast = ast;
            Value = value;

            ParserDiagnostics =
                parserDiagnostics
                ?? Array.Empty<string>();

            RuntimeDiagnostics =
                runtimeDiagnostics
                ?? Array.Empty<QslProvenanceEntry>();

            Exception = exception;
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