using QSL.Core;
using QSL.Core.Provenance;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

internal static class Program
{
    private static int Main(
        string[] args)
    {
        if (args.Length > 0)
        {
            if (string.Equals(
                    args[0],
                    "certify",
                    StringComparison.OrdinalIgnoreCase))
            {
                return CertificationRunner.Run();
            }

            if (string.Equals(
                    args[0],
                    "audit-ast-coverage",
                    StringComparison.OrdinalIgnoreCase))
            {
                return AstCoverageAuditRunner.Run();
            }

            Console.Error.WriteLine(
                "Unknown command: " +
                args[0]
            );

            Console.Error.WriteLine(
                "Available commands:"
            );

            Console.Error.WriteLine(
                "  certify"
            );

            Console.Error.WriteLine(
                "  audit-ast-coverage"
            );

            return 2;
        }

        RunInteractive();

        return 0;
    }

    private static void RunInteractive()
    {
        var host =
            QslCliHost.Create();

        WriteBanner();

        while (true)
        {
            Console.Write(
                "qsl> "
            );

            var source =
                Console.ReadLine();

            if (source is null)
            {
                Console.WriteLine();
                break;
            }

            source =
                source.Trim();

            if (source.Length == 0)
            {
                continue;
            }

            if (string.Equals(
                    source,
                    "exit",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    source,
                    "quit",
                    StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.Equals(
                    source,
                    "help",
                    StringComparison.OrdinalIgnoreCase))
            {
                WriteHelp();
                continue;
            }

            if (string.Equals(
                    source,
                    "clear",
                    StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                WriteBanner();
                continue;
            }

            if (string.Equals(
                    source,
                    "state",
                    StringComparison.OrdinalIgnoreCase))
            {
                WriteState(
                    host.Context
                );

                continue;
            }

            if (string.Equals(
                    source,
                    "provenance",
                    StringComparison.OrdinalIgnoreCase))
            {
                WriteProvenance();
                continue;
            }

            if (string.Equals(
                    source,
                    "certify",
                    StringComparison.OrdinalIgnoreCase))
            {
                var exitCode =
                    CertificationRunner.Run();

                Console.WriteLine();
                Console.WriteLine(
                    "Certification exit code: " +
                    exitCode
                );

                Console.WriteLine();
                continue;
            }

            Evaluate(
                host.Engine,
                source
            );
        }

        Console.WriteLine(
            "QSL CLI closed."
        );
    }

    private static void Evaluate(
        QslEngine engine,
        string source)
    {
        var result =
            engine.Evaluate(
                source
            );

        if (result.Succeeded)
        {
            Console.WriteLine(
                QslValueFormatter.Format(
                    result.Value
                )
            );

            Console.WriteLine();
            return;
        }

        Console.WriteLine(
            "Evaluation failed."
        );

        if (result.ParserDiagnostics.Count > 0)
        {
            Console.WriteLine(
                "Parser diagnostics:"
            );

            foreach (var diagnostic in
                     result.ParserDiagnostics)
            {
                Console.WriteLine(
                    "  " +
                    diagnostic
                );
            }
        }

        if (result.RuntimeDiagnostics.Count > 0)
        {
            Console.WriteLine(
                "Runtime diagnostics:"
            );

            foreach (var diagnostic in
                     result.RuntimeDiagnostics)
            {
                Console.WriteLine(
                    "  [" +
                    diagnostic.EntryType +
                    "] " +
                    diagnostic.Source +
                    ": " +
                    diagnostic.Message
                );
            }
        }

        if (result.Exception is not null)
        {
            Console.WriteLine(
                "Exception:"
            );

            Console.WriteLine(
                "  " +
                result.Exception.Message
            );
        }

        Console.WriteLine();
    }

    private static void WriteBanner()
    {
        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "QSL CLI"
        );

        Console.WriteLine(
            "Standalone deterministic QSL runtime"
        );

        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "Type a QSL expression and press Enter."
        );

        Console.WriteLine(
            "Type 'help' for available commands."
        );

        Console.WriteLine();
    }

    private static void WriteHelp()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Commands:"
        );

        Console.WriteLine(
            "  help        Show this help."
        );

        Console.WriteLine(
            "  state       Show current runtime variables."
        );

        Console.WriteLine(
            "  provenance  Show provenance entries."
        );

        Console.WriteLine(
            "  certify     Run the certification suite."
        );

        Console.WriteLine(
            "  clear       Clear the console."
        );

        Console.WriteLine(
            "  exit        Exit the CLI."
        );

        Console.WriteLine();
    }

    private static void WriteState(
        QslRuntimeContext context)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Runtime state:"
        );

        if (context.Variables.Count == 0)
        {
            Console.WriteLine(
                "  <empty>"
            );

            Console.WriteLine();
            return;
        }

        foreach (var pair in
                 context.Variables)
        {
            Console.WriteLine(
                "  " +
                pair.Key +
                " = " +
                QslValueFormatter.Format(
                    pair.Value
                )
            );
        }

        Console.WriteLine();
    }

    private static void WriteProvenance()
    {
        var entries =
            QslRuntime.Instance
                .ProvenanceBus
                .GetAll();

        Console.WriteLine();
        Console.WriteLine(
            "Provenance entries: " +
            entries.Count
        );

        if (entries.Count == 0)
        {
            Console.WriteLine(
                "  <none>"
            );

            Console.WriteLine();
            return;
        }

        for (
            var index = 0;
            index < entries.Count;
            index++)
        {
            var entry =
                entries[index];

            Console.WriteLine(
                "  " +
                (index + 1) +
                ". [" +
                entry.EntryType +
                "] [" +
                entry.Category +
                "] " +
                entry.Source +
                ": " +
                entry.Message
            );
        }

        Console.WriteLine();
    }
}