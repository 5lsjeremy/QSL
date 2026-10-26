using System;
using QSL.Abstractions.Runtime;
using QSL.Core;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

// ------------------------------------------------------------
// Consumer-owned descriptor
// ------------------------------------------------------------

public sealed class PlayerDescriptor :
    QslRegistryItem,
    IQslDescriptor
{
    public override string Name => "Player";
    public override string Type => "descriptor";

    public void Apply(
        QslRuntimeContext context)
    {
        context.SetVariable(
            "health",
            100
        );

        context.SetVariable(
            "mana",
            50
        );
    }
}

// ------------------------------------------------------------
// Consumer-owned function
// ------------------------------------------------------------

public sealed class HealFunction :
    QslRegistryItem,
    IQslFunction
{
    public override string Name => "heal";
    public override string Type => "function";

    public object? Invoke(
        object?[] args,
        QslRuntimeContext context)
    {
        if (args.Length != 1)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "heal requires exactly one argument."
            );

            return null;
        }

        var amount =
            Convert.ToDouble(args[0]);

        var currentHealth =
            Convert.ToDouble(
                context.ResolveIdentifier(
                    "health"
                ) ?? 0d
            );

        var updatedHealth =
            currentHealth + amount;

        context.SetVariable(
            "health",
            updatedHealth
        );

        return updatedHealth;
    }
}

// ------------------------------------------------------------
// Consumer host
// ------------------------------------------------------------

public static class Program
{
    public static void Main()
    {
        var registry =
            new QslRuntimeRegistry();

        registry.RegisterDefaultEvaluators();

        var player =
            new PlayerDescriptor();

        var heal =
            new HealFunction();

        registry.RegisterType(player);
        registry.RegisterType(heal);

        var context =
            new QslRuntimeContext(
                registry
            );

        player.Apply(context);

        var engine =
            new QslEngine(
                context
            );

        Console.WriteLine(
            "QSL Runtime 1.0 - Consumer Test 3"
        );

        Console.WriteLine();

        Run(
            engine,
            "health"
        );

        Run(
            engine,
            "heal(10)"
        );

        Run(
            engine,
            "health"
        );

        Run(
            engine,
            "health < 120 ? heal(5) : health"
        );

        Run(
            engine,
            "health"
        );
    }

    private static void Run(
        QslEngine engine,
        string source)
    {
        Console.WriteLine(
            $"> {source}"
        );

        var result =
            engine.Evaluate(
                source
            );

        if (result.Succeeded)
        {
            Console.WriteLine(
                $"  Result: {result.Value}"
            );

            Console.WriteLine();

            return;
        }

        foreach (
            var diagnostic
            in result.ParserDiagnostics)
        {
            Console.WriteLine(
                $"  Parser: {diagnostic}"
            );
        }

        foreach (
            var diagnostic
            in result.RuntimeDiagnostics)
        {
            Console.WriteLine(
                $"  Runtime: {diagnostic.Message}"
            );
        }

        if (result.Exception is not null)
        {
            Console.WriteLine(
                $"  Exception: {result.Exception.Message}"
            );
        }

        Console.WriteLine();
    }
}