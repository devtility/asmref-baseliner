// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.IO;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class WorkflowChecker
    {
        private const string WorkflowTarget = "CheckAsmRefWorkflow";
        private const string PublishTarget = "_PublishAsmRefBaselineFile";
        private const string CompareTarget = "_CompareAsmRefReportsOnBuild";
        private const string UpdateBaselineTarget = "_UpdateAsmRefBaselineFile";

        private readonly string projectDirectory;
        private readonly string BaselineFilePath;

        public WorkflowChecker(string projectDirectory, string projectName)
        {
            this.projectDirectory = projectDirectory;
            BaselineFilePath = Path.Combine(projectDirectory, $"AsmRef_{projectName}_Baseline.txt");
        }

        private string latestReportFilePath;
        private string LatestReportFilePath
        {
            get
            {
                if (latestReportFilePath == null)
                {
                    var matches = FindLatestReportFiles();
                    matches.Length.Should().Be(1);
                    latestReportFilePath = matches[0];
                }
                return latestReportFilePath;
            }
        }

        public void CheckReportsDoNotExist()
        {
            CheckBaselineExists(false);
            CheckLatestReportDoesNotExist();
        }

        public void CheckNoTargetsExecuted(BuildChecker logChecker)
        {
            logChecker.CheckTargetsNotExecuted(WorkflowTarget, CompareTarget, PublishTarget, UpdateBaselineTarget);
        }

        public void CheckBaselinePublished(BuildChecker logChecker)
        {
            logChecker.CheckTargetsSucceeded(WorkflowTarget, PublishTarget);
            logChecker.CheckTargetsNotExecuted(CompareTarget, UpdateBaselineTarget);

            CheckBaselineExists(true);
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
            var baselineContent = File.ReadAllText(BaselineFilePath);
            var latestContent = File.ReadAllText(LatestReportFilePath);

            baselineContent.Equals(latestContent, StringComparison.Ordinal)
                .Should().Be(expected);
        }

        private string[] FindLatestReportFiles()
            => Directory.GetFiles(projectDirectory, "AsmRef*Latest.txt", SearchOption.AllDirectories);

        private void CheckReportsExist()
        {
            CheckBaselineExists(true);
            File.Exists(LatestReportFilePath).Should().BeTrue();
        }

        private void CheckBaselineExists(bool shouldExist)
            => File.Exists(BaselineFilePath).Should().Be(shouldExist);

        private void CheckLatestReportDoesNotExist()
            => FindLatestReportFiles().Length.Should().Be(0);

    }
}
