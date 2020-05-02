// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal abstract class AbstractExeBuildRunner : IBuildRunner
    {
        protected AbstractExeBuildRunner(ITestOutputHelper logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected ITestOutputHelper Logger { get; }

        protected abstract string BinLogFileNamePrefix { get; }

        protected abstract string ExePath { get; }

        protected virtual void InitializeBuild() { /* no-op */ }

        protected abstract string BuildCommandLineArgs(string projectFilePath, string targetName,
            string binLogFilePath, Dictionary<string, string> additionalProperties);

        BuildChecker IBuildRunner.BuildSingleTarget(string projectFilePath, string targetName,
            Dictionary<string, string> additionalProperties)
        {
            InitializeBuild();

            var projectDir = Path.GetDirectoryName(projectFilePath);
            var binLogFilePath = Path.Combine(projectDir, $"{BinLogFileNamePrefix}.{targetName}.binlog");

            if (File.Exists(binLogFilePath))
            {
                WriteLine($"Deleting existing binlog file: {binLogFilePath}");
                File.Delete(binLogFilePath);
            }

            var args = BuildCommandLineArgs(projectFilePath, targetName, binLogFilePath, additionalProperties);
            WriteLine($"Command line arguments: {args}");

            var exeRunner = new ExeRunner(Logger);
            var executionResult = exeRunner.Run(ExePath, args);
            DumpExecutionResult(executionResult);

            File.Exists(binLogFilePath).Should().BeTrue();

            var buildSucceeded = executionResult.Status == ExeRunner.ExecutionStatus.Completed
                && executionResult.ExitCode == 0;

            var buildChecker = new BuildChecker(buildSucceeded, binLogFilePath);
            return buildChecker;
        }

        private void DumpExecutionResult(ExeRunner.ExecutionResult result)
        {
            var standardOutput = result.StandardOutput;
            var standardError = result.StandardError;
            if (string.IsNullOrEmpty(standardOutput))
            {
                standardOutput = "{empty}";
            }
            if (string.IsNullOrEmpty(standardError))
            {
                standardError = "{empty}";
            }

            WriteLine($"Exit code: {result.ExitCode}");
            WriteLine($"Execution status: {result.Status}");
            WriteLine($"Standard output: {standardOutput}");
            WriteLine($"Standard error: {standardError}");
            WriteLine(string.Empty);
            WriteLine(string.Empty);
        }

        protected void WriteLine(string message) => Logger.WriteLine(message);
    }
}
