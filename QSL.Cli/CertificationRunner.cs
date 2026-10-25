using System.Text.Json;
using QSL.Core;
using QSL.Core.Provenance;

internal static class CertificationRunner
{
    private const string SuiteFileName =
        "qsl_statement_testing.json";

    private const string ReportFileName =
        "qsl_statement_testing_output.json";

    public static int Run()
    {
        var suitePath =
            Path.Combine(
                AppContext.BaseDirectory,
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
            var json =
                File.ReadAllText(
                    suitePath
                );

            suite =
                JsonSerializer.Deserialize
                    <QslCertificationSuite>(
                        json,
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

        var report =
            new QslCertificationReport();

        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "QSL CERTIFICATION"
        );

        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "Suite: " +
            suitePath
        );

        Console.WriteLine(
            "Tests: " +
            suite.Tests.Count
        );

        Console.WriteLine();

        for (
            var index = 0;
            index < suite.Tests.Count;
            index++)
        {
            var test =
                suite.Tests[index];

            var result =
                ExecuteTest(
                    host.Engine,
                    test
                );

            report.Results.Add(
                result
            );

            if (result.Passed)
            {
                report.Passed++;
            }
            else
            {
                report.Failed++;
            }

            Console.Write(
                result.Passed
                    ? "."
                    : "F"
            );

            if ((index + 1) % 50 == 0)
            {
                Console.WriteLine();
            }
        }

        if (suite.Tests.Count % 50 != 0)
        {
            Console.WriteLine();
        }

        report.Total =
            report.Results.Count;

        Console.WriteLine();
        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "Total:  " +
            report.Total
        );

        Console.WriteLine(
            "Passed: " +
            report.Passed
        );

        Console.WriteLine(
            "Failed: " +
            report.Failed
        );

        Console.WriteLine(
            "========================================"
        );

        if (report.Failed > 0)
        {
            WriteFailures(
                report.Results
            );
        }

        var reportPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                ReportFileName
            );

        try
        {
            var reportJson =
                JsonSerializer.Serialize(
                    report,
                    new JsonSerializerOptions
                    {
                        WriteIndented =
                            true
                    }
                );

            File.WriteAllText(
                reportPath,
                reportJson
            );

            Console.WriteLine();
            Console.WriteLine(
                "Report:"
            );

            Console.WriteLine(
                reportPath
            );
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(
                "Certification report could not be written."
            );

            Console.Error.WriteLine(
                exception.Message
            );

            return 3;
        }

        return report.Failed == 0
            ? 0
            : 1;
    }

    private static QslCertificationResult ExecuteTest(
        QslEngine engine,
        QslCertificationTest test)
    {
        var processingResult =
            engine.Evaluate(
                test.Statement
            );

        var output =
            QslValueFormatter.Format(
                processingResult.Value
            );

        var expectsError =
            string.Equals(
                test.Category,
                "Error",
                StringComparison.OrdinalIgnoreCase
            );

        var passed =
            expectsError
                ? !processingResult.Succeeded
                : processingResult.Succeeded &&
                  string.Equals(
                      output,
                      test.Expected,
                      StringComparison.Ordinal
                  );

        var runtimeDiagnostics =
            new string[
                processingResult
                    .RuntimeDiagnostics
                    .Count
            ];

        for (
            var index = 0;
            index < runtimeDiagnostics.Length;
            index++)
        {
            runtimeDiagnostics[index] =
                FormatRuntimeDiagnostic(
                    processingResult
                        .RuntimeDiagnostics[index]
                );
        }

        var parserDiagnostics =
            new string[
                processingResult
                    .ParserDiagnostics
                    .Count
            ];

        for (
            var index = 0;
            index < parserDiagnostics.Length;
            index++)
        {
            parserDiagnostics[index] =
                processingResult
                    .ParserDiagnostics[index];
        }

        return new QslCertificationResult
        {
            Category =
                test.Category,

            Statement =
                test.Statement,

            Expected =
                test.Expected,

            Output =
                output,

            Passed =
                passed,

            Succeeded =
                processingResult.Succeeded,

            ParseFailed =
                processingResult.ParseFailed,

            EvaluationFailed =
                processingResult.EvaluationFailed,

            ParserDiagnostics =
                parserDiagnostics,

            RuntimeDiagnostics =
                runtimeDiagnostics,

            Exception =
                processingResult
                    .Exception?
                    .Message
        };
    }

    private static string FormatRuntimeDiagnostic(
        QslProvenanceEntry entry)
    {
        return
            "[" +
            entry.EntryType +
            "] " +
            entry.Source +
            ": " +
            entry.Message;
    }

    private static void WriteFailures(
        IReadOnlyList<QslCertificationResult>
            results)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Failures:"
        );

        foreach (var result in results)
        {
            if (result.Passed)
            {
                continue;
            }

            Console.WriteLine();
            Console.WriteLine(
                "[" +
                result.Category +
                "] " +
                result.Statement
            );

            Console.WriteLine(
                "  Expected: " +
                result.Expected
            );

            Console.WriteLine(
                "  Actual:   " +
                (
                    string.IsNullOrEmpty(
                        result.Output
                    )
                        ? "<null>"
                        : result.Output
                )
            );

            Console.WriteLine(
                "  Succeeded: " +
                result.Succeeded
            );

            foreach (var diagnostic in
                     result.ParserDiagnostics)
            {
                Console.WriteLine(
                    "  Parser: " +
                    diagnostic
                );
            }

            foreach (var diagnostic in
                     result.RuntimeDiagnostics)
            {
                Console.WriteLine(
                    "  Runtime: " +
                    diagnostic
                );
            }

            if (!string.IsNullOrEmpty(
                    result.Exception))
            {
                Console.WriteLine(
                    "  Exception: " +
                    result.Exception
                );
            }
        }
    }
}