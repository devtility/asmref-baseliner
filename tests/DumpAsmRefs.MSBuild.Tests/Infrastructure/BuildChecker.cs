// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Logging.StructuredLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class BuildChecker
    {
        private readonly Build buildRoot;

        private readonly IList<Target> allTargets;
        private readonly IList<Task> allTasks;

        public BuildChecker(bool succeeded, string binLogFilePath)
        {
            OverallBuildSucceeded = succeeded;
            buildRoot = BinaryLog.ReadBuild(binLogFilePath);

            allTargets = new List<Target>();
            buildRoot.VisitAllChildren<Target>(t => allTargets.Add(t));

            allTasks = new List<Task>();
            buildRoot.VisitAllChildren<Task>(t => allTasks.Add(t));
        }

        public bool OverallBuildSucceeded { get; }

        public Target FindSingleTargetExecution(string targetName)
        {
            var matches = allTargets.Where(t => IsTargetByName(t, targetName));
            matches.Count().Should().Be(1);
            return matches.First();
        }

        public IDictionary<string, string> GetPropertyAssignmentsInTarget(string targetName)
        {
            var target = FindSingleTargetExecution(targetName);
            if (target == null)
            {
                return null;
            }

            const string propertyAssignmentPattern = ".+: (.+)=(.*)";
            return GetPropertyAssignmentsInNode(target, propertyAssignmentPattern);
        }

        public IDictionary<string, string> GetTaskInputs(string taskName)
        {
            var task = allTasks.FirstOrDefault(t => t.Name == taskName);
            if (task == null)
            {
                return null;
            }

            const string inputPropertyPattern = ".+ :(.+)=(.*)";
            return GetPropertyAssignmentsInNode(task, inputPropertyPattern);
        }

        private static IDictionary<string, string> GetPropertyAssignmentsInNode(TreeNode node, string pattern)
        {
            // Property assignments are not stored directly in the log. Instead, there
            // will be a message saying "Property set: aaa=123".
            // Also, the same property could be set multiple times in the target. If we
            // wanted all of the interim assignments we could store them in a list.
            var map = new Dictionary<string, string>();

            node.VisitAllChildren<Message>(tryGetPropertyValue);
            return map;

            void tryGetPropertyValue(Message message)
            {
                // Example: Property set/redefined : AsmRefIncludePatterns=workflow.dll
                var match = Regex.Match(message.Text, pattern);

                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    map[key] = value;
                }
            }
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
