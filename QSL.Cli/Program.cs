using System;
using System.Globalization;
using QSL.Core;
using QSL.Core.Provenance;
using QSL.Core.Runtime;

internal static class Program
{
    private static void Main()
    {
        var registry =
            new QslRuntimeRegistry();

        registry.RegisterDefaultEvaluators();

        registry.RegisterType(
            new HealFunction()
        );

        registry.RegisterType(
            new VectorFunction()
        );

        var physicsCapability =
            new PhysicsCapability();

        registry.RegisterType(
            physicsCapability
        );

        var playerDescriptor =
            new PlayerDescriptor();

        registry.RegisterType(
            playerDescriptor
        );

        var context =
            new QslRuntimeContext(
                registry
            );

        physicsCapability.Apply(
            context
        );

        playerDescriptor.Apply(
            context
        );

        var engine =
            new QslEngine(
                context
            );

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
                    context
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

            Evaluate(
                engine,
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
                FormatValue(
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
            "  provenance  Show all provenance entries."
        );

        Console.WriteLine(
            "  clear       Clear the console."
        );

        Console.WriteLine(
            "  exit        Exit the CLI."
        );

        Console.WriteLine();
        Console.WriteLine(
            "Examples:"
        );

        Console.WriteLine(
            "  1 + 2"
        );

        Console.WriteLine(
            "  health"
        );

        Console.WriteLine(
            "  health = 75"
        );

        Console.WriteLine(
            "  heal(10)"
        );

        Console.WriteLine(
            "  Vector(10, 20)"
        );

        Console.WriteLine(
            "  Vector(10, 20, 30)"
        );

        Console.WriteLine(
            "  ApplyForce(\"Player\", Vector(10, 0))"
        );

        Console.WriteLine(
            "  \"Health: $(health)\""
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
                FormatValue(
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

    private static string FormatValue(
        object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is bool boolean)
        {
            return boolean
                ? "true"
                : "false";
        }

        if (value is double number)
        {
            return number.ToString(
                "0.################",
                CultureInfo.InvariantCulture
            );
        }

        if (value is float single)
        {
            return single.ToString(
                "0.################",
                CultureInfo.InvariantCulture
            );
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(
                       null,
                       CultureInfo.InvariantCulture
                   )
                   ?? string.Empty;
        }

        return value.ToString()
               ?? string.Empty;
    }
}