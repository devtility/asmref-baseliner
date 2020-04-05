// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Logging.StructuredLogger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class BuildLogChecker
    {
        private readonly Build buildRoot;

        private readonly IList<Target> allTargets;

        public BuildLogChecker(string binLogFilePath)
        {
            buildRoot = BinaryLog.ReadBuild(binLogFilePath);

            allTargets = new List<Target>();
            buildRoot.VisitAllChildren<Target>(t => allTargets.Add(t));
        }

        public Target FindSingleTargetExecution(string targetName)
        {
            var matches = allTargets.Where(t => IsTargetByName(t, targetName));
            matches.Count().Should().Be(1);
            return matches.First();
        }

        private static bool IsTargetByName(Target target, string name)
            => string.Equals(target.Name, name, StringComparison.Ordinal);
    }
}
