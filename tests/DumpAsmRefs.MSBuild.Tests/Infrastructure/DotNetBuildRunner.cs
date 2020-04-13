// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class DotNetBuildRunner : IBuildRunner
    {
        private const string exePath = "dotnet";

        private readonly ITestOutputHelper output;

        public DotNetBuildRunner(ITestOutputHelper output)
        {
            this.output = output;
        }

        BuildChecker IBuildRunner.BuildSingleTarget(string projectFilePath, string targetName, Dictionary<string, string> additionalProperties)
        {
            DumpDotNetVersion();

            var projectDir = Path.GetDirectoryName(projectFilePath);
            var binLogFilePath = Path.Combine(projectDir, $"msbuild.{targetName}.binlog");

            var sb = new StringBuilder();
            sb.Append($"build \"{projectFilePath}\"");
            sb.Append($" -t:{targetName} ");
            sb.Append($" -bl:LogFile=\"{binLogFilePath}\"");

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    sb.Append($" -p:{kvp.Key}=\"{kvp.Value}\"");
                }
            }

            var args = sb.ToString();
            WriteLine($"Command line arguments: {args}");
            var executionResult = ExeRunner.Run(exePath, args);
            DumpExecutionResult(executionResult);

            File.Exists(binLogFilePath).Should().BeTrue();

            var buildSucceeded = executionResult.Status == ExeRunner.ExecutionStatus.Completed
                && executionResult.ExitCode == 0;

            var buildChecker = new BuildChecker(buildSucceeded, binLogFilePath);
            return buildChecker;
        }

        private void DumpDotNetVersion()
        {
            var executionResult = ExeRunner.Run(exePath, "--version");
            WriteLine($"dotnet version: {executionResult.StandardOutput}");
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

        private void WriteLine(string message)
        {
            output.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
