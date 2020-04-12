// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    public class DotNetBuildTests
    {
        private readonly ITestOutputHelper output;

        static DotNetBuildTests()
        {
            // Must be done in a separate method, before any code that uses the
            // Microsoft.Build namespace.
            // See https://github.com/microsoft/MSBuildLocator/commit/f3d5b0814bc7c5734d03a617c17c6998dd2f0e99
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
        }

        public DotNetBuildTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SimpleBuild()
        {
            var context = TestContext.Initialize(output);

            const string proj = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>
</Project>
";

            const string code = @"
using System;

namespace MyNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}";
            var projFilePath = context.WriteFile("myApp.csproj", proj, "proj1");
            context.WriteFile("program.cs", code, "proj1");

            BuildSingleTarget(projFilePath, "Restore");
            var (buildResult, buildChecker) = BuildSingleTarget(projFilePath, "Build");

            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            buildChecker.FindSingleTargetExecution("Compile")
                .Succeeded.Should().BeTrue();
        }

        [Fact]
        public void RestorePackageThenBuild()
        {
            var context = TestContext.Initialize(output);

            var proj = $@"<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <RestorePackagesPath>{context.LocalNuGetFeedPath}</RestorePackagesPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include='Devtility.CheckAsmRefs' Version='{context.PackageVersion}' />
  </ItemGroup>
</Project>";

            const string code = @"
using System;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""Hello World!"");
    }
}";
            var projectFilePath = context.WriteFile("myApp.csproj", proj, "proj1");
            context.WriteFile("program.cs", code, "proj1");
            
            var checker = new WorkflowChecker(Path.GetDirectoryName(projectFilePath), "myApp");

            // 1. Restore
            var (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Restore");
            buildResult.OverallResult.Should().Be(BuildResultCode.Success);
            checker.CheckNoTargetsExecuted(logChecker);
            checker.CheckReportsDoNotExist();

            // 2. Build -> baseline file created
            (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Build");
            buildResult.OverallResult.Should().Be(BuildResultCode.Success);
            checker.CheckBaselinePublished(logChecker);

            // 3. Build again -> comparison run, no error
            (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Build");
            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            checker.CheckComparisonExecutedAndSucceeded(logChecker);
            checker.CheckReportsAreDifferent();

            // 4. Add new ref, build -> comparison run, build fails
            const string newCode = @"
class Class1
{
    void Method1(System.Data.AcceptRejectRule arg1) { /* no-op */ }
}";
            context.WriteFile("newCode.cs", newCode, "proj1");
            (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Build");
            buildResult.OverallResult.Should().Be(BuildResultCode.Failure);

            checker.CheckComparisonExecutedAndFailed(logChecker);
            checker.CheckReportsAreDifferent();

            // 5. Update -> baseline file updated
            var properties = new Dictionary<string, string>
            {
                { "AsmRefUpdateBaseline", "true"}
            };
            (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Build", properties);
            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            checker.CheckBaselineUpdatePerformed(logChecker);
            checker.CheckReportsAreSame();

            // 6. Build again -> comparison run, no error
            (buildResult, logChecker) = BuildSingleTarget(projectFilePath, "Build");
            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            checker.CheckComparisonExecutedAndSucceeded(logChecker);
            checker.CheckReportsAreDifferent();
        }

        private static (BuildResult, BuildLogChecker) BuildSingleTarget(string projectFilePath, string targetName,
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
            var buildChecker = new BuildLogChecker(binLogFilePath);
            return (buildResult, buildChecker);
        }
    }
}
