using System.Globalization;
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

            if (source.StartsWith(
                    "state-add ",
                    StringComparison.OrdinalIgnoreCase))
            {
                ReceiveState(
                    host.Context,
                    source
                );

                continue;
            }

            if (source.StartsWith(
                    "state-remove ",
                    StringComparison.OrdinalIgnoreCase))
            {
                RemoveState(
                    host.Context,
                    source
                );

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

    private static void ReceiveState(
        QslRuntimeContext context,
        string source)
    {
        var parts =
            source.Split(
                new[]
                {
                    ' '
                },
                4,
                StringSplitOptions.RemoveEmptyEntries
            );

        if (parts.Length != 4)
        {
            Console.WriteLine(
                "Usage: state-add <target> <property> <value>"
            );

            Console.WriteLine();
            return;
        }

        var targetName =
            parts[1];

        var propertyName =
            parts[2];

        var value =
            ParseStateValue(
                parts[3]
            );

        var target =
            context.ResolveIdentifier(
                targetName
            );

        if (target is null)
        {
            Console.WriteLine(
                $"Target '{targetName}' could not be resolved."
            );

            Console.WriteLine();
            return;
        }

        var received =
            context.ReceiveState(
                target,
                propertyName,
                value
            );

        if (!received)
        {
            Console.WriteLine(
                $"State '{propertyName}' already exists " +
                $"for target '{targetName}'."
            );

            Console.WriteLine();
            return;
        }

        Console.WriteLine(
            $"State received: " +
            $"{targetName}.{propertyName} = " +
            QslValueFormatter.Format(
                value
            )
        );

        Console.WriteLine();
    }

    private static void RemoveState(
        QslRuntimeContext context,
        string source)
    {
        var parts =
            source.Split(
                new[]
                {
                    ' '
                },
                StringSplitOptions.RemoveEmptyEntries
            );

        if (parts.Length != 2 &&
            parts.Length != 3)
        {
            Console.WriteLine(
                "Usage: state-remove <target> [property]"
            );

            Console.WriteLine();
            return;
        }

        var targetName =
            parts[1];

        var target =
            context.ResolveIdentifier(
                targetName
            );

        if (target is null)
        {
            Console.WriteLine(
                $"Target '{targetName}' could not be resolved."
            );

            Console.WriteLine();
            return;
        }

        if (parts.Length == 2)
        {
            var removed =
                context.RemoveState(
                    target
                );

            Console.WriteLine(
                removed
                    ? $"All attached state removed from '{targetName}'."
                    : $"Target '{targetName}' has no attached state."
            );

            Console.WriteLine();
            return;
        }

        var propertyName =
            parts[2];

        var propertyRemoved =
            context.RemoveState(
                target,
                propertyName
            );

        Console.WriteLine(
            propertyRemoved
                ? $"State removed: {targetName}.{propertyName}"
                : $"State '{propertyName}' does not exist " +
                  $"for target '{targetName}'."
        );

        Console.WriteLine();
    }

    private static object? ParseStateValue(
        string source)
    {
        if (string.Equals(
                source,
                "null",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (bool.TryParse(
                source,
                out var boolean))
        {
            return boolean;
        }

        if (double.TryParse(
                source,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number))
        {
            return number;
        }

        if (source.Length >= 2)
        {
            if ((source[0] == '"' &&
                 source[source.Length - 1] == '"') ||
                (source[0] == '\'' &&
                 source[source.Length - 1] == '\''))
            {
                return source.Substring(
                    1,
                    source.Length - 2
                );
            }
        }

        return source;
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
            "  help                            Show this help."
        );

        Console.WriteLine(
            "  state                           Show runtime variables."
        );

        Console.WriteLine(
            "  state-add <target> <property> <value>"
        );

        Console.WriteLine(
            "                                  Receive attached state."
        );

        Console.WriteLine(
            "  state-remove <target> <property>"
        );

        Console.WriteLine(
            "                                  Remove attached state."
        );

        Console.WriteLine(
            "  state-remove <target>           Remove all attached state."
        );

        Console.WriteLine(
            "  provenance                      Show provenance entries."
        );

        Console.WriteLine(
            "  certify                         Run certification."
        );

        Console.WriteLine(
            "  clear                           Clear the console."
        );

        Console.WriteLine(
            "  exit                            Exit the CLI."
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