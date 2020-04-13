// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal interface IBuildRunner
    {
        BuildChecker BuildSingleTarget(string projectFilePath, string targetName,
                Dictionary<string, string> additionalProperties = null);
    }

    internal static class BuildRunnerExtensions
    {
        public static BuildChecker Build(this IBuildRunner buildRunner, string projectFileName,
            Dictionary<string, string> additionalProperties = null) =>
            buildRunner.BuildSingleTarget(projectFileName, "Build", additionalProperties);

        public static BuildChecker Restore(this IBuildRunner buildRunner, string projectFileName,
            Dictionary<string, string> additionalProperties = null) =>
            buildRunner.BuildSingleTarget(projectFileName, "Restore", additionalProperties);
    }
}
