// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class MSBuildRunner : IBuildRunner
    {
        static MSBuildRunner()
        {
            MSBuildLocatorInitializer.EnsureMSBuildInitialized();
        }

        BuildChecker IBuildRunner.BuildSingleTarget(string projectFilePath, string targetName,
                        Dictionary<string, string> additionalProperties)
        {
            var projectDir = Path.GetDirectoryName(projectFilePath);
            var binLogFilePath = Path.Combine(projectDir, $"msbuild.{targetName}.binlog");

            var buildParams = new BuildParameters
            {
                Loggers = new ILogger[] { new Microsoft.Build.Logging.BinaryLogger { Parameters = binLogFilePath } }
            };

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParams,
                new BuildRequestData(projectFilePath,
                    additionalProperties ?? new Dictionary<string, string>(),
                    null,
                    new[] { targetName },
                    null,
                    BuildRequestDataFlags.None));

            File.Exists(binLogFilePath).Should().BeTrue();

            var buildChecker = new BuildChecker(buildResult.OverallResult == BuildResultCode.Success, binLogFilePath);
            return buildChecker;
        }
    }
}
