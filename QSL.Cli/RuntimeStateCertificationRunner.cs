using System;
using System.Collections.Generic;
using System.Linq;
using QSL.Core;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;

internal static class RuntimeStateCertificationRunner
{
    public static QslRuntimeStateCertificationReport Run()
    {
        var report =
            new QslRuntimeStateCertificationReport();

        RunCase(report, "Consumer binding resolves exact runtime object", TestExactBinding);
        RunCase(report, "Consumer can be unbound", TestUnbind);
        RunCase(report, "Stale binding cannot remove replacement", TestStaleBinding);
        RunCase(report, "Attached state is absent before receipt", TestAbsentBeforeReceipt);
        RunCase(report, "Single receipt introduces arbitrary vocabulary", TestSingleReceipt);
        RunCase(report, "Duplicate receipt preserves existing state", TestDuplicateReceipt);
        RunCase(report, "Update changes existing attached state", TestUpdateExisting);
        RunCase(report, "Update cannot introduce missing state", TestUpdateMissing);
        RunCase(report, "QSL assignment updates attached state", TestAttachedAssignment);
        RunCase(report, "Native and attached values compose", TestNativeAttachedComposition);
        RunCase(report, "State package introduces multiple properties", TestPackageReceipt);
        RunCase(report, "State package preserves existing properties", TestPackagePreservation);
        RunCase(report, "Decay package updates and removes state", TestDecayUpdateAndRemoval);
        RunCase(report, "Decay cannot introduce state and can preserve zero", TestDecayBoundaries);
        RunCase(report, "Package provenance and clear lifecycle are preserved", TestProvenanceAndClear);

        report.Total =
            report.Results.Count;

        return report;
    }

    private static void RunCase(
        QslRuntimeStateCertificationReport report,
        string name,
        Action test)
    {
        try
        {
            test();

            report.Results.Add(
                new QslRuntimeStateCertificationResult
                {
                    Name = name,
                    Passed = true,
                    Details = "Certified."
                }
            );

            report.Passed++;
        }
        catch (Exception exception)
        {
            report.Results.Add(
                new QslRuntimeStateCertificationResult
                {
                    Name = name,
                    Passed = false,
                    Details = exception.Message,
                    Exception = exception.ToString()
                }
            );

            report.Failed++;
        }
    }

    private static void TestExactBinding()
    {
        var host = QslCliHost.Create();
        var resolved = host.Context.ResolveIdentifier("player");

        Require(
            ReferenceEquals(resolved, host.Player.Value),
            "The player identifier did not resolve to the exact bound object."
        );
    }

    private static void TestUnbind()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(
            host.Player.Value,
            "Curiosity",
            7.0
        );

        Require(
            host.Engine.UnbindConsumer(host.Player),
            "The exact consumer binding was not removed."
        );

        Require(
            host.Context.GetVariable("player") is null,
            "The player identifier remained available after unbinding."
        );

        Require(
            !host.Context.AttachedState.Contains(
                host.Player.Value,
                "Curiosity"),
            "Unbinding did not retire attached state."
        );
    }

    private static void TestStaleBinding()
    {
        var host = QslCliHost.Create();
        var replacement = new object();
        var replacementBinding =
            new QslConsumerBinding(
                "player",
                "Entity",
                replacement
            );

        host.Engine.BindConsumer(replacementBinding);

        Require(
            !host.Engine.UnbindConsumer(host.Player),
            "A stale binding removed a replacement object."
        );

        Require(
            ReferenceEquals(
                replacement,
                host.Context.GetVariable("player")),
            "The replacement binding was not preserved."
        );
    }

    private static void TestAbsentBeforeReceipt()
    {
        var host = QslCliHost.Create();
        var result = host.Engine.Evaluate("player.Curiosity");

        Require(
            !result.Succeeded,
            "An unattached member resolved before receipt."
        );

        Require(
            !host.Context.AttachedState.Contains(
                host.Player.Value,
                "Curiosity"),
            "The failed lookup created attached state."
        );
    }

    private static void TestSingleReceipt()
    {
        var host = QslCliHost.Create();

        Require(
            host.Engine.ReceiveState(
                host.Player.Value,
                "Curiosity",
                7.0),
            "The state was not received."
        );

        RequireEvaluation(
            host.Engine,
            "player.Curiosity",
            "7"
        );
    }

    private static void TestDuplicateReceipt()
    {
        var host = QslCliHost.Create();

        Require(
            host.Engine.ReceiveState(
                host.Player.Value,
                "Curiosity",
                7.0),
            "The initial state was not received."
        );

        Require(
            !host.Engine.ReceiveState(
                host.Player.Value,
                "Curiosity",
                99.0),
            "Duplicate receipt incorrectly overwrote existing state."
        );

        RequireEvaluation(
            host.Engine,
            "player.Curiosity",
            "7"
        );
    }

    private static void TestUpdateExisting()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(
            host.Player.Value,
            "Curiosity",
            7.0
        );

        Require(
            host.Engine.UpdateState(
                host.Player.Value,
                "Curiosity",
                3.5),
            "Existing attached state was not updated."
        );

        RequireEvaluation(
            host.Engine,
            "player.Curiosity",
            "3.5"
        );
    }

    private static void TestUpdateMissing()
    {
        var host = QslCliHost.Create();

        Require(
            !host.Engine.UpdateState(
                host.Player.Value,
                "Curiosity",
                3.5),
            "Update introduced missing attached state."
        );

        Require(
            !host.Context.AttachedState.Contains(
                host.Player.Value,
                "Curiosity"),
            "Missing state appeared after a rejected update."
        );
    }

    private static void TestAttachedAssignment()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(
            host.Player.Value,
            "Curiosity",
            7.0
        );

        RequireEvaluation(
            host.Engine,
            "player.Curiosity = player.Curiosity + 3",
            "10"
        );

        RequireEvaluation(
            host.Engine,
            "player.Curiosity",
            "10"
        );
    }

    private static void TestNativeAttachedComposition()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(
            host.Action.Value,
            "Amount",
            10.0
        );

        host.Engine.ReceiveState(
            host.Action.Value,
            "SpaceDustAppetite",
            87.0
        );

        RequireEvaluation(
            host.Engine,
            "player.Health = player.Health - " +
            "(action.Amount * " +
            "(action.SpaceDustAppetite / 100))",
            "41.3"
        );

        RequireEvaluation(
            host.Engine,
            "player.Health",
            "41.3"
        );
    }

    private static void TestPackageReceipt()
    {
        var host = QslCliHost.Create();
        var package =
            new QslStatePackage(
                host.Action.Value,
                new Dictionary<string, object?>
                {
                    ["Amount"] = 10.0,
                    ["SpaceDustAppetite"] = 87.0
                }
            );

        Require(
            host.Engine.ReceiveState(package) == 2,
            "The state package did not report two received properties."
        );

        RequireEvaluation(host.Engine, "action.Amount", "10");
        RequireEvaluation(host.Engine, "action.SpaceDustAppetite", "87");
    }

    private static void TestPackagePreservation()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(
            host.Action.Value,
            "Amount",
            10.0
        );

        var package =
            new QslStatePackage(
                host.Action.Value,
                new Dictionary<string, object?>
                {
                    ["Amount"] = 99.0,
                    ["SpaceDustAppetite"] = 87.0
                }
            );

        Require(
            host.Engine.ReceiveState(package) == 1,
            "The package affected an unexpected number of properties."
        );

        RequireEvaluation(host.Engine, "action.Amount", "10");
        RequireEvaluation(host.Engine, "action.SpaceDustAppetite", "87");
    }

    private static void TestDecayUpdateAndRemoval()
    {
        var host = QslCliHost.Create();

        host.Engine.ReceiveState(host.Player.Value, "Curiosity", 7.0);
        host.Engine.ReceiveState(host.Player.Value, "Interest", 5.0);

        var package =
            new QslDecayPackage(
                host.Player.Value,
                new Dictionary<string, object?>
                {
                    ["Curiosity"] = 3.5,
                    ["Interest"] = 0.0
                }
            );

        Require(
            host.Engine.ReceiveDecay(package) == 2,
            "The decay package did not report two affected properties."
        );

        RequireEvaluation(host.Engine, "player.Curiosity", "3.5");

        Require(
            !host.Context.AttachedState.Contains(
                host.Player.Value,
                "Interest"),
            "Zero-valued decay did not retire the state."
        );
    }

    private static void TestDecayBoundaries()
    {
        var host = QslCliHost.Create();

        var missing =
            new QslDecayPackage(
                host.Player.Value,
                new Dictionary<string, object?>
                {
                    ["Unknown"] = 4.0
                }
            );

        Require(
            host.Engine.ReceiveDecay(missing) == 0,
            "Decay introduced missing state."
        );

        host.Engine.ReceiveState(
            host.Player.Value,
            "Interest",
            5.0
        );

        var preserveZero =
            new QslDecayPackage(
                host.Player.Value,
                new Dictionary<string, object?>
                {
                    ["Interest"] = 0.0
                },
                removeAtZero: false
            );

        Require(
            host.Engine.ReceiveDecay(preserveZero) == 1,
            "Zero was not applied when removal was disabled."
        );

        RequireEvaluation(host.Engine, "player.Interest", "0");
    }

    private static void TestProvenanceAndClear()
    {
        var host = QslCliHost.Create();
        var package =
            new QslStatePackage(
                host.Player.Value,
                new Dictionary<string, object?>
                {
                    ["Curiosity"] = 7.0,
                    ["Interest"] = 5.0
                }
            );

        Require(
            package.Id != Guid.Empty,
            "The package did not receive an identity."
        );

        Require(
            !string.IsNullOrWhiteSpace(package.Slug),
            "The package did not receive a slug."
        );

        Require(
            host.Engine.ReceiveState(package) == 2,
            "The package was not fully applied."
        );

        Require(
            package.Provenance.Any(
                entry =>
                    entry.Message ==
                    "QSL package created."),
            "Package creation provenance was not recorded."
        );

        Require(
            package.Provenance.Any(
                entry =>
                    entry.Message ==
                    "QSL package applied."),
            "Package application provenance was not recorded."
        );

        var applied =
            package.Provenance.Last(
                entry =>
                    entry.Message ==
                    "QSL package applied."
            );

        Require(
            applied.Metadata.TryGetValue(
                "affected",
                out var affected) &&
            Convert.ToInt32(affected) == 2,
            "Package provenance did not record the affected count."
        );

        host.Context.SetMetadata("scope", "certification");
        host.Engine.Clear();

        Require(
            host.Context.Variables.Count == 0,
            "Clear did not remove runtime variables."
        );

        Require(
            host.Context.Metadata.Count == 0,
            "Clear did not remove runtime metadata."
        );

        Require(
            !host.Context.AttachedState.Contains(
                host.Player.Value,
                "Curiosity"),
            "Clear did not remove attached state."
        );

        RequireEvaluation(host.Engine, "10 + 20", "30");
    }

    private static void RequireEvaluation(
        QslEngine engine,
        string statement,
        string expected)
    {
        var result = engine.Evaluate(statement);
        var output = QslValueFormatter.Format(result.Value);

        Require(
            result.Succeeded,
            "QSL evaluation failed for: " + statement
        );

        Require(
            string.Equals(
                output,
                expected,
                StringComparison.Ordinal),
            "QSL output mismatch for '" +
            statement +
            "'. Expected '" +
            expected +
            "' but received '" +
            output +
            "'."
        );
    }

    private static void Require(
        bool condition,
        string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
