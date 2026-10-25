using System.Globalization;
using System.Text;
using System.Text.Json;
using QSL.Core.Ast.Types;

internal static class AstCoverageAuditRunner
{
    private const string SuiteFileName =
        "qsl_statement_testing.json";

    public static int Run()
    {
        var rootPath =
            FindRepositoryRoot();

        if (rootPath is null)
        {
            Console.Error.WriteLine(
                "Could not locate the QSL repository root."
            );

            return 2;
        }

        var suitePath =
            Path.Combine(
                rootPath,
                "QSL.Cli",
                "Certification",
                SuiteFileName
            );

        if (!File.Exists(suitePath))
        {
            Console.Error.WriteLine(
                "Certification suite was not found:"
            );

            Console.Error.WriteLine(
                suitePath
            );

            return 2;
        }

        QslCertificationSuite? suite;

        try
        {
            var suiteJson =
                File.ReadAllText(
                    suitePath
                );

            suite =
                JsonSerializer.Deserialize
                    <QslCertificationSuite>(
                        suiteJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        }
                    );
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                "Certification suite could not be loaded."
            );

            Console.Error.WriteLine(
                exception.Message
            );

            return 2;
        }

        if (suite is null ||
            suite.Tests.Count == 0)
        {
            Console.Error.WriteLine(
                "Certification suite contains no tests."
            );

            return 2;
        }

        var host =
            QslCliHost.Create();

        var allKinds =
            Enum.GetValues<QslNodeKind>();

        var totalOccurrences =
            new Dictionary<QslNodeKind, int>();

        var statementCounts =
            new Dictionary<QslNodeKind, int>();

        foreach (var kind in allKinds)
        {
            totalOccurrences.Add(
                kind,
                0
            );

            statementCounts.Add(
                kind,
                0
            );
        }

        var report =
            new AstCoverageReport
            {
                GeneratedUtc =
                    DateTime.UtcNow.ToString(
                        "O",
                        CultureInfo.InvariantCulture
                    ),

                SuitePath =
                    suitePath,

                TotalStatements =
                    suite.Tests.Count,

                DefinedKindCount =
                    allKinds.Length
            };

        for (
            var index = 0;
            index < suite.Tests.Count;
            index++)
        {
            var test =
                suite.Tests[index];

            var parseResult =
                host.Engine.Parse(
                    test.Statement
                );

            var walkResult =
                AstWalker.Walk(
                    parseResult.Ast
                );

            report.ParsedStatements++;
            report.TotalAstNodes +=
                walkResult.NodeCount;

            var encounteredKinds =
                walkResult.KindCounts.Keys
                    .OrderBy(kind => kind.ToString())
                    .ToArray();

            foreach (var pair in
                     walkResult.KindCounts)
            {
                totalOccurrences[pair.Key] +=
                    pair.Value;

                statementCounts[pair.Key]++;
            }

            report.Tests.Add(
                new AstTestCoverageRecord
                {
                    Index =
                        index + 1,

                    Category =
                        test.Category,

                    Statement =
                        test.Statement,

                    RootKind =
                        parseResult.Ast.Kind.ToString(),

                    NodeCount =
                        walkResult.NodeCount,

                    Kinds =
                        encounteredKinds
                            .Select(kind => kind.ToString())
                            .ToArray(),

                    ParserDiagnostics =
                        parseResult.Diagnostics.ToArray()
                }
            );
        }

        foreach (var kind in allKinds)
        {
            var occurrences =
                totalOccurrences[kind];

            var covered =
                occurrences > 0;

            report.Kinds.Add(
                new AstKindCoverageRecord
                {
                    Kind =
                        kind.ToString(),

                    Occurrences =
                        occurrences,

                    TestCount =
                        statementCounts[kind],

                    Covered =
                        covered
                }
            );
        }

        report.CoveredKindCount =
            report.Kinds.Count(
                item => item.Covered
            );

        report.UncoveredKindCount =
            report.DefinedKindCount -
            report.CoveredKindCount;

        report.Complete =
            report.UncoveredKindCount == 0;

        var reportsPath =
            Path.Combine(
                rootPath,
                "Audit",
                "Reports"
            );

        try
        {
            Directory.CreateDirectory(
                reportsPath
            );

            WriteReports(
                reportsPath,
                report
            );
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                "AST coverage reports could not be written."
            );

            Console.Error.WriteLine(
                exception.Message
            );

            return 3;
        }

        WriteConsoleSummary(
            report,
            reportsPath
        );

        return report.Complete
            ? 0
            : 1;
    }

    private static void WriteReports(
        string reportsPath,
        AstCoverageReport report)
    {
        var timestamp =
            DateTime.Now.ToString(
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture
            );

        var timestampedTextPath =
            Path.Combine(
                reportsPath,
                $"QSL_AST_Coverage_{timestamp}.txt"
            );

        var timestampedJsonPath =
            Path.Combine(
                reportsPath,
                $"QSL_AST_Coverage_{timestamp}.json"
            );

        var latestTextPath =
            Path.Combine(
                reportsPath,
                "QSL_AST_Coverage_Latest.txt"
            );

        var latestJsonPath =
            Path.Combine(
                reportsPath,
                "QSL_AST_Coverage_Latest.json"
            );

        var textReport =
            BuildTextReport(
                report
            );

        var jsonReport =
            JsonSerializer.Serialize(
                report,
                new JsonSerializerOptions
                {
                    WriteIndented =
                        true
                }
            );

        File.WriteAllText(
            timestampedTextPath,
            textReport
        );

        File.WriteAllText(
            timestampedJsonPath,
            jsonReport
        );

        File.WriteAllText(
            latestTextPath,
            textReport
        );

        File.WriteAllText(
            latestJsonPath,
            jsonReport
        );
    }

    private static string BuildTextReport(
        AstCoverageReport report)
    {
        var builder =
            new StringBuilder();

        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            "QSL AST CERTIFICATION COVERAGE"
        );

        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine();
        builder.AppendLine(
            $"Generated UTC:          {report.GeneratedUtc}"
        );

        builder.AppendLine(
            $"Suite:                  {report.SuitePath}"
        );

        builder.AppendLine(
            $"Certification statements: {report.TotalStatements}"
        );

        builder.AppendLine(
            $"Parsed statements:        {report.ParsedStatements}"
        );

        builder.AppendLine(
            $"AST nodes visited:        {report.TotalAstNodes}"
        );

        builder.AppendLine(
            $"Node kinds defined:       {report.DefinedKindCount}"
        );

        builder.AppendLine(
            $"Node kinds covered:       {report.CoveredKindCount}"
        );

        builder.AppendLine(
            $"Node kinds uncovered:     {report.UncoveredKindCount}"
        );

        builder.AppendLine();
        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            "KIND COVERAGE"
        );

        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            "Kind".PadRight(24) +
            "Occurrences".PadLeft(14) +
            "Tests".PadLeft(10) +
            "Covered".PadLeft(12)
        );

        builder.AppendLine(
            new string(
                '-',
                60
            )
        );

        foreach (var kind in report.Kinds)
        {
            builder.AppendLine(
                kind.Kind.PadRight(24) +
                kind.Occurrences
                    .ToString(
                        CultureInfo.InvariantCulture
                    )
                    .PadLeft(14) +
                kind.TestCount
                    .ToString(
                        CultureInfo.InvariantCulture
                    )
                    .PadLeft(10) +
                (
                    kind.Covered
                        ? "YES"
                        : "NO"
                ).PadLeft(12)
            );
        }

        var uncovered =
            report.Kinds
                .Where(item => !item.Covered)
                .ToArray();

        builder.AppendLine();
        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            "UNCOVERED KINDS"
        );

        builder.AppendLine(
            "============================================================"
        );

        if (uncovered.Length == 0)
        {
            builder.AppendLine(
                "<none>"
            );
        }
        else
        {
            foreach (var kind in uncovered)
            {
                builder.AppendLine(
                    kind.Kind
                );
            }
        }

        builder.AppendLine();
        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            "SUMMARY"
        );

        builder.AppendLine(
            "============================================================"
        );

        builder.AppendLine(
            $"Statements:       {report.TotalStatements}"
        );

        builder.AppendLine(
            $"AST nodes:        {report.TotalAstNodes}"
        );

        builder.AppendLine(
            $"Kinds covered:    {report.CoveredKindCount}/{report.DefinedKindCount}"
        );

        builder.AppendLine(
            $"Kinds uncovered:  {report.UncoveredKindCount}"
        );

        builder.AppendLine();

        builder.AppendLine(
            report.Complete
                ? "STATUS: AST COVERAGE COMPLETE"
                : "STATUS: AST COVERAGE INCOMPLETE"
        );

        builder.AppendLine();
        builder.AppendLine(
            "No QSL source files were modified by this audit."
        );

        return builder.ToString();
    }

    private static void WriteConsoleSummary(
        AstCoverageReport report,
        string reportsPath)
    {
        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "QSL AST COVERAGE AUDIT"
        );

        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            $"Statements:       {report.TotalStatements}"
        );

        Console.WriteLine(
            $"Parsed:           {report.ParsedStatements}"
        );

        Console.WriteLine(
            $"AST nodes:        {report.TotalAstNodes}"
        );

        Console.WriteLine(
            $"Kinds covered:    {report.CoveredKindCount}/{report.DefinedKindCount}"
        );

        Console.WriteLine(
            $"Kinds uncovered:  {report.UncoveredKindCount}"
        );

        Console.WriteLine();

        foreach (var kind in report.Kinds)
        {
            Console.WriteLine(
                kind.Kind.PadRight(24) +
                kind.Occurrences
                    .ToString(
                        CultureInfo.InvariantCulture
                    )
                    .PadLeft(8) +
                " nodes  " +
                kind.TestCount
                    .ToString(
                        CultureInfo.InvariantCulture
                    )
                    .PadLeft(4) +
                " tests"
            );
        }

        Console.WriteLine();

        Console.WriteLine(
            report.Complete
                ? "STATUS: AST COVERAGE COMPLETE"
                : "STATUS: AST COVERAGE INCOMPLETE"
        );

        Console.WriteLine();
        Console.WriteLine(
            "Reports:"
        );

        Console.WriteLine(
            reportsPath
        );
    }

    private static string? FindRepositoryRoot()
    {
        var candidates =
            new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

        foreach (var candidate in candidates)
        {
            var current =
                new DirectoryInfo(
                    candidate
                );

            while (current is not null)
            {
                var solutionPath =
                    Path.Combine(
                        current.FullName,
                        "QSL.sln"
                    );

                var projectPath =
                    Path.Combine(
                        current.FullName,
                        "QSL.csproj"
                    );

                if (File.Exists(solutionPath) &&
                    File.Exists(projectPath))
                {
                    return current.FullName;
                }

                current =
                    current.Parent;
            }
        }

        return null;
    }
}