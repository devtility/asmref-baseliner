// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Logging.StructuredLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class BuildChecker
    {
        private readonly Build buildRoot;

        private readonly IList<Target> allTargets;
        private readonly IList<Task> allTasks;
        private readonly ITestOutputHelper logger;
        private readonly bool overallBuildSucceeded;

        public BuildChecker(bool succeeded, string binLogFilePath, ITestOutputHelper logger)
        {
            overallBuildSucceeded = succeeded;
            buildRoot = BinaryLog.ReadBuild(binLogFilePath);
            this.logger = logger;

            allTargets = new List<Target>();
            buildRoot.VisitAllChildren<Target>(t => allTargets.Add(t));

            allTasks = new List<Task>();
            buildRoot.VisitAllChildren<Task>(t => allTasks.Add(t));
        }

        public Target FindSingleTargetExecution(string targetName)
        {
            var matches = allTargets.Where(t => IsTargetByName(t, targetName));
            matches.Count().Should().Be(1, $"target {targetName} should have executed once and only once. Actual times: {matches.Count()}");
            return matches.First();
        }

        public IDictionary<string, string> GetPropertyAssignmentsInTarget(string targetName)
        {
            Log($"Getting property assignments in target '{targetName}':");
            var target = FindSingleTargetExecution(targetName);
            if (target == null)
            {
                LogIndent("-> target not found");
                return null;
            }

            const string propertyAssignmentPattern = ".+: (.+)=(.*)";
            var result = GetPropertyAssignmentsInNode(target, propertyAssignmentPattern);
            return result;
        }

        public IDictionary<string, string> GetTaskInputs(string taskName)
        {
            Log($"Getting inputs for task '{taskName}':");
            var task = allTasks.FirstOrDefault(t => t.Name == taskName);
            if (task == null)
            {
                LogIndent("-> task not found");
                return null;
            }

            const string inputPropertyPattern = ".+ :(.+)=(.*)";
            var result = GetPropertyAssignmentsInNode(task, inputPropertyPattern);
            return result;
        }

        private IDictionary<string, string> GetPropertyAssignmentsInNode(TreeNode node, string pattern)
        {
            IDictionary<string, string> map = new Dictionary<string, string>();

            LogIndent("Looking for properties...");
            node.VisitAllChildren<Property>(p => map[p.Name] = p.Value);
            if (map.Count == 0)
            {
                LogIndent("No children of type Property found. Falling back on parsing messages to find properties...");
                map = FindPropertyAssignmentsInMessages(node, pattern);
            }

            LogIndent($"-> count={map.Count}");
            return map;
        }

        private IDictionary<string, string> FindPropertyAssignmentsInMessages(TreeNode node, string pattern)
        {
            // The binary log stores text strings describing operations e.g. "Property set: x=y".
            // The MS binary log reader parses the messages and extracts the data to turn them into
            // strongly-typed objects like "Property".
            // Unfortunately, the text stored in the log is localised, but the MS binary log reader
            // assumes the entries are in en-us. So it won't correctly identify e.g. property assignment
            // log entries if the log is written in a different language.
            // If that happens, we'll end up here and we'll try to extract property assignments from
            // the log Messages ourselves.
            // Also, the same property could be set multiple times in the target. If we
            // wanted all of the interim assignments we could store them in a list.
            var map = new Dictionary<string, string>();

            LogIndent("Looking for properties in Messages...");
            LogIndent($"RegEx: \"{pattern}\"");
            node.VisitAllChildren<Message>(tryGetPropertyValueFromMessage);

            return map;

            void tryGetPropertyValueFromMessage(Message message)
            {
                LogIndent($"Message: {message}");
                // Example: Property set/redefined : AsmRefIncludePatterns=workflow.dll
                var match = Regex.Match(message.Text, pattern);

                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    map[key] = value;
                    LogIndent($"  -> {key}={value}");
                }
                else
                {
                    LogIndent($"  -> property assignment not found");
                }
            }
        }

        public void CheckBuildSucceeded(string messageSuffix = null)
        {
            Log($"Checking build succeeded... {messageSuffix}");
            overallBuildSucceeded.Should().BeTrue();
            LogOk();
        }

        public void CheckBuildFailed(string messageSuffix = null)
        {
            Log($"Checking build failed... {messageSuffix}");
            overallBuildSucceeded.Should().BeFalse();
            LogOk();
        }

        public void CheckTargetsFailed(params string[] targetNames)
            => CheckTargetsSuccess(false, targetNames);

        public void CheckTargetsSucceeded(params string[] targetNames)
            => CheckTargetsSuccess(true, targetNames);

        public void CheckTargetsNotExecuted(params string[] targetNames)
        {
            foreach(var item in targetNames)
            {
                Log($"Checking target '{item}' was not executed...");
                var matches = allTargets.Where(t => IsTargetByName(t, item));
                matches.Count().Should().Be(0, $"target {item} should not have executed");
                LogOk();
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
        {
            Log($"Checking target outcome: {targetName}, expected: {expected}");

            FindSingleTargetExecution(targetName).Succeeded.Should().Be(expected,
                $"target {targetName} should have {(expected ? "succeeded" : "failed")}");

            LogOk();
        }

        private static bool IsTargetByName(Target target, string name)
            => string.Equals(target.Name, name, StringComparison.Ordinal);

        private void Log(string message) => logger.WriteLine($"Build log checker: {message}");
        private void LogIndent(string message) => Log($"  {message}");
        private void LogOk() => LogIndent($"-> ok");
    }
}
