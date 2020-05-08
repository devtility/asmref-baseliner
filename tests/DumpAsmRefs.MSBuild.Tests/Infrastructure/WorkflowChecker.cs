// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class WorkflowChecker
    {
        private const string WorkflowTarget = "CheckAsmRefWorkflow";
        private const string PublishTarget = "_PublishAsmRefBaselineFile";
        private const string CompareTarget = "_CompareAsmRefReportsOnBuild";
        private const string UpdateBaselineTarget = "_UpdateAsmRefBaselineFile";

        private const string InputValidationTargetName = "_EnsureAsmRefFilePathsAreSet";

        private const string ComparisonTaskName = "CompareAsmRefReportFiles";

        private readonly string projectDirectory;
        private readonly string baselineFilePath;
        private readonly ITestOutputHelper logger;

        public WorkflowChecker(string projectDirectory, string projectName, ITestOutputHelper logger)
        {
            this.projectDirectory = projectDirectory;
            baselineFilePath = Path.Combine(projectDirectory, $"AsmRef_{projectName}_Baseline.txt");
            this.logger = logger;
        }

        public TargetsInputs GetPropertiesSetInInputValidationTarget(BuildChecker buildChecker)
        {
            var targetInputs = new TargetsInputs();

            var msbuildProperties = buildChecker.GetPropertyAssignmentsInTarget(InputValidationTargetName);
            SetObjectProperties(targetInputs, msbuildProperties, false);

            return targetInputs;
        }

        public ComparisonTaskInputs GetCompareTaskInputs(BuildChecker buildChecker)
        {
            var inputs = buildChecker.GetTaskInputs(ComparisonTaskName);
            if (inputs == null)
            {
                throw new InvalidOperationException($"Test error: could not find task in the log. Task name: {ComparisonTaskName}");
            }

            var taskInputs = new ComparisonTaskInputs();
            SetObjectProperties(taskInputs, inputs, true);

            return taskInputs;
        }

        private static void SetObjectProperties(object instance, IDictionary<string, string> values, bool throwIfNotFound)
        {
            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (!values.TryGetValue(property.Name, out string data) && throwIfNotFound)
                {
                    throw new InvalidOperationException($"Test error: could not find expected property in log. Property name: {property.Name}");
                }
                property.SetValue(instance, data);
            }
        }

        public TargetsInputs GetPropertiesSetInInputValidationTarget(BuildChecker buildChecker)
        {
            var targetInputs = new TargetsInputs();

            var msbuildProperties = buildChecker.GetPropertyAssignmentsInTarget(InputValidationTargetName);
            SetObjectProperties(targetInputs, msbuildProperties, false);

            return targetInputs;
        }

        public ComparisonTaskInputs GetCompareTaskInputs(BuildChecker buildChecker)
        {
            var inputs = buildChecker.GetTaskInputs(ComparisonTaskName);
            if (inputs == null)
            {
                throw new InvalidOperationException($"Test error: could not find task in the log. Task name: {ComparisonTaskName}");
            }

            var taskInputs = new ComparisonTaskInputs();
            SetObjectProperties(taskInputs, inputs, true);

            return taskInputs;
        }

        private static void SetObjectProperties(object instance, IDictionary<string, string> values, bool throwIfNotFound)
        {
            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (!values.TryGetValue(property.Name, out string data) && throwIfNotFound)
                {
                    throw new InvalidOperationException($"Test error: could not find expected property in log. Property name: {property.Name}");
                }
                property.SetValue(instance, data);
            }
        }

        public void CheckReportsDoNotExist()
        {
            CheckBaselineDoesNotExist();
            CheckLatestReportDoesNotExist();
        }

        public void CheckNoTargetsExecuted(BuildChecker logChecker)
        {
            logChecker.CheckTargetsNotExecuted(WorkflowTarget, CompareTarget, PublishTarget, UpdateBaselineTarget);
        }

        public void CheckBaselinePublished(BuildChecker logChecker)
        {
            Log("Checking Baseline publishing...");

            logChecker.CheckTargetsSucceeded(WorkflowTarget, PublishTarget);
            logChecker.CheckTargetsNotExecuted(CompareTarget, UpdateBaselineTarget);

            CheckBaselineExists();
            CheckLatestReportDoesNotExist();
        }

        public void CheckComparisonExecutedAndSucceeded(BuildChecker logChecker)
        {
            logChecker.CheckTargetsSucceeded(WorkflowTarget, CompareTarget);
            logChecker.CheckTargetsNotExecuted(PublishTarget, UpdateBaselineTarget);

            CheckReportsExist();
        }

        public void CheckComparisonExecutedAndFailed(BuildChecker logChecker)
        {
            logChecker.CheckTargetsFailed(WorkflowTarget, CompareTarget);
            logChecker.CheckTargetsNotExecuted(PublishTarget, UpdateBaselineTarget);

            CheckReportsExist();
        }

        public void CheckBaselineUpdatePerformed(BuildChecker logChecker)
        {
            logChecker.CheckTargetsSucceeded(WorkflowTarget, UpdateBaselineTarget);
            logChecker.CheckTargetsNotExecuted(CompareTarget, PublishTarget);

            CheckReportsExist();
        }

        public void CheckReportsAreSame()
            => CheckReportContents(true);

        public void CheckReportsAreDifferent()
            => CheckReportContents(false);

        private void CheckReportContents(bool expected)
        {
            var baselineContent = File.ReadAllText(CheckBaselineExists());
            var latestContent = File.ReadAllText(CheckLatestReportExists());

            baselineContent.Equals(latestContent, StringComparison.Ordinal)
                .Should().Be(expected, "reports contents should match");
        }

        private string[] FindLatestReportFiles()
        {
            Log($"  Searching for latest report files in {projectDirectory}");
            var reports = Directory.GetFiles(projectDirectory, "AsmRef*Latest.txt", SearchOption.AllDirectories);

            var reportsList = (reports.Length == 0) ? "{none}" : string.Join(", ", reports);
            Log($"  -> Matches: {reportsList}");
            return reports;
        }

        private void CheckReportsExist()
        {
            CheckBaselineExists();
            CheckLatestReportExists();
        }

        private string CheckBaselineExists()
        {
            Log($"Checking baseline exists at ${baselineFilePath}");
            File.Exists(baselineFilePath).Should().BeTrue($"baseline file should exist at {baselineFilePath}");
            LogOk();
            return baselineFilePath;
        }

        private void CheckBaselineDoesNotExist()
        {
            Log($"Checking baseline does not exist at {baselineFilePath}");
            File.Exists(baselineFilePath).Should().BeFalse($"baseline file should not exist at {baselineFilePath}");
            LogOk();
        }

        private string CheckLatestReportExists()
        {
            Log("Checking latest report exists...");
            var reports = FindLatestReportFiles();
            reports.Length.Should().Be(1);
            LogOk();
            return reports[0];
        }

        private void CheckLatestReportDoesNotExist()
        {
            Log("Checking latest report does not exist...");
            var reports = FindLatestReportFiles();
            reports.Length.Should().Be(0);
            LogOk();
        }

        private void Log(string message) => logger.WriteLine($"Workflow checker: {message}");

        private void LogOk() => Log($"  -> ok");
    }
}
