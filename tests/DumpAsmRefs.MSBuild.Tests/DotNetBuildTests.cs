// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
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

        [Theory]
        [InlineData("msbuild")]
        [InlineData("dotnet")]
        public void SimpleBuild(string buildRunnerId)
        {
            var buildRunner = CreateBuildRunner(buildRunnerId);

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

            buildRunner.Restore(projFilePath);
            var buildChecker = buildRunner.Build(projFilePath);

            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            buildChecker.FindSingleTargetExecution("Compile")
                .Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData("msbuild")]
        [InlineData("dotnet")]
        public void WorkflowLifecycle(string buildRunnerId)
        {
            var buildRunner = CreateBuildRunner(buildRunnerId);

            var context = TestContext.Initialize(output);

            var proj = $@"<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <RestorePackagesPath>{context.NuGetPackageCachePath}</RestorePackagesPath>
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
            var buildChecker = buildRunner.Restore(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();
            workflowChecker.CheckNoTargetsExecuted(buildChecker);
            workflowChecker.CheckReportsDoNotExist();

            // 2. Build -> baseline file created
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();
            workflowChecker.CheckBaselinePublished(buildChecker);

            // 3. Build again -> comparison run, no error
            buildChecker = buildRunner.Build(projectFilePath);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            workflowChecker.CheckComparisonExecutedAndSucceeded(buildChecker);
            workflowChecker.CheckReportsAreDifferent();

            // 4. Add new ref, build -> comparison run, build fails
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
            var properties = new Dictionary<string, string>
            {
                { "AsmRefUpdateBaseline", "true"}
            };
            buildChecker = buildRunner.Build(projectFilePath, properties);
            buildChecker.OverallBuildSucceeded.Should().BeTrue();

            workflowChecker.CheckBaselineUpdatePerformed(buildChecker);
            workflowChecker.CheckReportsAreSame();

            // 6. Build again -> comparison run, no error
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
                    return new MSBuildRunner();
                case "dotnet":
                    return new DotNetBuildRunner(output);
                default:
                    throw new System.ArgumentException($"Test setup error - unrecognised runner id: {runnerId}");
            }
        }
    }
}
