// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class MSBuildRunner
    {
        static MSBuildRunner()
        {
            // Must be done in a separate method, before any code that uses the
            // Microsoft.Build namespace.
            // See https://github.com/microsoft/MSBuildLocator/commit/f3d5b0814bc7c5734d03a617c17c6998dd2f0e99
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
        }

        public BuildChecker Build(string projectFilePath, string targetName,
                        Dictionary<string, string> additionalProperties = null)
        {
            return BuildSingleTarget(projectFilePath, targetName, additionalProperties);
        }

        private static BuildChecker BuildSingleTarget(string projectFilePath, string targetName,
            Dictionary<string, string> additionalProperties = null)
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
