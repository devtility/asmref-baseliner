// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    public class DotNetBuildTests
    {
        private readonly ITestOutputHelper output;

        public DotNetBuildTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [SkippableTheory(typeof(NotSupportedException))]
        [InlineData("msbuild")]
        [InlineData("dotnet")]
        public void SimpleBuild(string buildRunnerId)
        {
            var buildRunner = CreateBuildRunner(buildRunnerId);

            var context = TestContext.Initialize(output, uniqueTestName: $"{nameof(SimpleBuild)}_{buildRunnerId}");

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

            buildRunner.Restore(projFilePath);
            var buildChecker = buildRunner.Build(projFilePath);

            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            buildChecker.FindSingleTargetExecution("Compile")
                .Succeeded.Should().BeTrue();
        }

        [SkippableTheory(typeof(NotSupportedException))]
        [InlineData("msbuild")]
        [InlineData("dotnet")]
        public void WorkflowLifecycle(string buildRunnerId)
        {
            var buildRunner = CreateBuildRunner(buildRunnerId);

            var context = TestContext.Initialize(output, uniqueTestName: $"{nameof(WorkflowLifecycle)}_{buildRunnerId}");

            var proj = $@"<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <RestorePackagesPath>{context.NuGetPackageCachePath}</RestorePackagesPath>
    <SonarQubeTargetsImported>true</SonarQubeTargetsImported>
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
            
            var workflowChecker = new WorkflowChecker(Path.GetDirectoryName(projectFilePath), "myApp");

            // 1. Restore
            LogTestStep("1 - initial restore -> not expecting a build or comparison");
            var buildChecker = buildRunner.Restore(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();
            workflowChecker.CheckNoTargetsExecuted(buildChecker);
            workflowChecker.CheckReportsDoNotExist();

            // 2. Build -> baseline file created
            LogTestStep("2 - initial build -> expecting baseline baseline to be created");
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();
            workflowChecker.CheckBaselinePublished(buildChecker);

            // 3. Build again -> comparison run, no error
            LogTestStep("3 - build again -> expecting comparison to run and succeed");
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            workflowChecker.CheckComparisonExecutedAndSucceeded(buildChecker);
            workflowChecker.CheckReportsAreDifferent();

            // 4. Add new ref, build -> comparison run, build fails
            LogTestStep("4 - add new ref then build -> expecting comparison to run and fail");
            const string newCode = @"
class Class1
{
    void Method1(System.Data.AcceptRejectRule arg1) { /* no-op */ }
}";
            context.WriteFile("newCode.cs", newCode, "proj1");
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeFalse();

            workflowChecker.CheckComparisonExecutedAndFailed(buildChecker);
            workflowChecker.CheckReportsAreDifferent();

            // 5. Update -> baseline file updated
            LogTestStep("5 - run with baseline option -> expecting success");
            var properties = new Dictionary<string, string>
            {
                { "AsmRefUpdateBaseline", "true"}
            };
            buildChecker = buildRunner.Build(projectFilePath, properties);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            workflowChecker.CheckBaselineUpdatePerformed(buildChecker);
            workflowChecker.CheckReportsAreSame();

            // 6. Build again -> comparison run, no error
            LogTestStep("6 - build again -> expecting comparison to run with no error");
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            workflowChecker.CheckComparisonExecutedAndSucceeded(buildChecker);
            workflowChecker.CheckReportsAreDifferent();
        }

        private IBuildRunner CreateBuildRunner(string runnerId)
        {
            switch(runnerId)
            {
                case "msbuild":
                    return new MSBuildExeRunner(output);
                case "dotnet":
                    return new DotNetBuildRunner(output);
                default:
                    throw new System.ArgumentException($"Test setup error - unrecognised runner id: {runnerId}");
            }
        }

        private void LogTestStep(string message)
        {
            output.WriteLine("");
            output.WriteLine("******************************************************************************************************");
            output.WriteLine($"*** Test step: {message}");
            output.WriteLine("******************************************************************************************************");
            output.WriteLine("");
        }
    }
}
