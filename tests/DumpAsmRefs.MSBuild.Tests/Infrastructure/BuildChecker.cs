// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Logging.StructuredLogger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class BuildChecker
    {
        private readonly Build buildRoot;

        private readonly IList<Target> allTargets;

        public BuildChecker(bool succeeded, string binLogFilePath)
        {
            OverallBuildSucceeded = succeeded;
            buildRoot = BinaryLog.ReadBuild(binLogFilePath);

            allTargets = new List<Target>();
            buildRoot.VisitAllChildren<Target>(t => allTargets.Add(t));
        }

        public bool OverallBuildSucceeded { get; }

        public Target FindSingleTargetExecution(string targetName)
        {
            var matches = allTargets.Where(t => IsTargetByName(t, targetName));
            matches.Count().Should().Be(1);
            return matches.First();
        }

        public void CheckTargetsFailed(params string[] targetNames)
            => CheckTargetsSuccess(false, targetNames);

        public void CheckTargetsSucceeded(params string[] targetNames)
            => CheckTargetsSuccess(true, targetNames);

        public void CheckTargetsNotExecuted(params string[] targetNames)
        {
            foreach(var item in targetNames)
            {
                var matches = allTargets.Where(t => IsTargetByName(t, item));
                matches.Count().Should().Be(0);
            }
        }

        private void CheckTargetsSuccess(bool expected, params string[] targetNames)
        {
            foreach (var item in targetNames)
            {
                CheckTargetOutcome(item, expected);
            }
        }

        private void CheckTargetOutcome(string targetName, bool expected)
            => FindSingleTargetExecution(targetName).Succeeded.Should().Be(expected);

        private static bool IsTargetByName(Target target, string name)
            => string.Equals(target.Name, name, StringComparison.Ordinal);
    }
}
