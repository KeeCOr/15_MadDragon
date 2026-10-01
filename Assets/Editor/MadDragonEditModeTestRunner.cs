using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class MadDragonEditModeTestRunner
{
    private const string DefaultResultPath = "C:/Development/15_MD/Logs/EditModeResults_MadDragon_Codex.xml";

    public static void Run()
    {
        string resultPath = GetArgumentValue("-mdTestResults", DefaultResultPath);
        string filter = GetArgumentValue("-mdTestFilter", string.Empty);

        var callbacks = new ResultCallbacks(resultPath);
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(callbacks);

        var testFilter = new Filter
        {
            testMode = TestMode.EditMode
        };

        if (!string.IsNullOrWhiteSpace(filter))
            testFilter.groupNames = filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        var settings = new ExecutionSettings(testFilter)
        {
            runSynchronously = true
        };

        Debug.Log("[MadDragonTestRunner] Starting EditMode tests with filter: " + filter);
        api.Execute(settings);
        api.UnregisterCallbacks(callbacks);
        UnityEngine.Object.DestroyImmediate(api);

        if (!callbacks.Finished)
        {
            Debug.LogError("[MadDragonTestRunner] Test run did not finish.");
            EditorApplication.Exit(2);
            return;
        }

        Debug.Log("[MadDragonTestRunner] " + callbacks.Summary);
        EditorApplication.Exit(callbacks.FailCount == 0 ? 0 : 1);
    }

    private static string GetArgumentValue(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }

        return fallback;
    }

    private sealed class ResultCallbacks : ICallbacks
    {
        private readonly string _resultPath;

        public bool Finished { get; private set; }
        public int FailCount { get; private set; }
        public string Summary { get; private set; }

        public ResultCallbacks(string resultPath)
        {
            _resultPath = resultPath;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log("[MadDragonTestRunner] RunStarted: " + testsToRun.FullName + " cases=" + testsToRun.TestCaseCount);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            Finished = true;
            FailCount = result.FailCount;
            Summary = string.Format(
                "RunFinished: passed={0} failed={1} skipped={2} inconclusive={3} asserts={4} duration={5:0.000}s result={6}",
                result.PassCount,
                result.FailCount,
                result.SkipCount,
                result.InconclusiveCount,
                result.AssertCount,
                result.Duration,
                result.ResultState);

            string directory = Path.GetDirectoryName(_resultPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_resultPath, result.ToXml().OuterXml);
            Debug.Log("[MadDragonTestRunner] Wrote XML: " + _resultPath);
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.Test.IsSuite)
                Debug.Log("[MadDragonTestRunner] TestFinished: " + result.FullName + " => " + result.ResultState);
        }
    }
}
