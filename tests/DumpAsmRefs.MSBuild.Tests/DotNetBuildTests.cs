// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
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
            CreateTestSpecificDirectory();

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
            var projFilePath = WriteTextFile("proj1", "myApp.csproj", proj);
            WriteTextFile("proj1", "program.cs", code);

            var (buildResult, buildChecker) = RestoreAndBuild(projFilePath);

            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            buildChecker.FindSingleTargetExecution("Compile")
                .Succeeded.Should().BeTrue();
        }

        [Fact]
        public void RestorePackageThenBuild()
        {
            var version = GetPackageVersion();
            var packagePath = GetTestAssemblyBinPath();

            CreateTestSpecificDirectory();

            var proj = $@"<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>

<!--
    <RestoreNoCache>true</RestoreNoCache>
    <RestoreForce>true</RestoreForce>
-->
    <RestorePackagesPath>{GetNuGetPackageCachePath()}</RestorePackagesPath>

  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include='Devtility.CheckAsmRefs' Version='{version}' />
  </ItemGroup>

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

            var nugetConfigContent = $@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <packageSources>
        <clear /> <!-- ensure only the sources defined below are used -->
        <add key='latestPackageFolder' value='{packagePath}' />
        <add key='NuGet official package source' value='https://api.nuget.org/v3/index.json' />
    </packageSources>
</configuration>
";

            var projFilePath = WriteTextFile("proj1", "myApp.csproj", proj);
            WriteTextFile("proj1", "program.cs", code);
            WriteTextFile("", "nuget.config", nugetConfigContent);

            var (buildResult, buildChecker) = RestoreAndBuild(projFilePath);

            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            buildChecker.FindSingleTargetExecution("Compile")
                .Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Build_NoBaseline_BaselineCreated()
        {
        }

        [Fact]
        public void Build_BaselineExists_NoChanges_TaskSucceeds()
        {
        }

        [Fact]
        public void Build_BaselineExists_Changes_TaskFails()
        {
        }

        [Fact]
        public void Build_UpdateFlagSet_BaselineUpdated()
        {
        }

        private string CreateTestSpecificDirectory([System.Runtime.CompilerServices.CallerMemberName] string subDirName = "")
        {
            var directory = Path.Combine(GetTestResultsPath(), subDirName);
            output.WriteLine($"Test-specific directory: {directory}");

            SafeDeleteDirectory(directory);

            Directory.CreateDirectory(directory);
            Directory.SetCurrentDirectory(directory);
            return directory;
        }

        private static void SafeDeleteDirectory(string directory)
        {
            var attempts = 0;
            while (attempts < 3 && Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (IOException)
                {
                    // ignore
                }
                attempts++;
            }
        }

        private static string GetTestAssemblyBinPath()
        {
            var uriCodeBase = typeof(DotNetBuildTests).Assembly.CodeBase;
            var uri = new Uri(uriCodeBase);
            var path = uri.AbsolutePath;
            return Path.GetDirectoryName(path);
        }

        private static string GetTestResultsPath()
        {
            const string folderName = "\\DumpAsmRefs.MSBuild.Tests\\";
            var projectBinPath = GetTestAssemblyBinPath();

            var index = projectBinPath.IndexOf(folderName);
            var projectDirectory = projectBinPath.Substring(0, index);
            return Path.Combine(projectDirectory, "TestResults");
        }

        private static string GetNuGetPackageCachePath()
            => Path.Combine(GetTestResultsPath(), "TestPackagesCache");

        private static string GetPackageVersion()
        {
            const string filePrefix = "Devtility.CheckAsmRefs.";
            var directory = GetTestAssemblyBinPath();
            var files = Directory.GetFiles(directory, $"{filePrefix}*.nupkg");

            if (files.Length != 1)
            {
                throw new InvalidOperationException("Test setup error: failed to locate the current NuGet package");
            }

            var version = Path.GetFileNameWithoutExtension(files[0]).Replace(filePrefix, "");
            return version;
        }

        private static string WriteTextFile(string subdir, string fileName, string text)
        {
            if (!string.IsNullOrEmpty(subdir) && !Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

            var fullPathName = Path.Combine(Environment.CurrentDirectory, subdir, fileName);
            File.WriteAllText(fullPathName, text);
            return fullPathName;
        }

        private static (BuildResult, BuildLogChecker) RestoreAndBuild(string projectFilePath)
        {
            BuildSingleTarget(projectFilePath, "Restore");
            var (buildResult, buildChecker) = BuildSingleTarget(projectFilePath, "Build");

            return (buildResult, buildChecker);
        }

        private static (BuildResult, BuildLogChecker) BuildSingleTarget(string projectFilePath, string targetName)
        {
            var projectDir = Path.GetDirectoryName(projectFilePath);
            var binLogFilePath = Path.Combine(projectDir, $"msbuild.{targetName}.binlog");

            var buildParams = new BuildParameters
            {
                Loggers = new ILogger[] { new Microsoft.Build.Logging.BinaryLogger { Parameters = binLogFilePath } }
            };

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParams,
                new BuildRequestData(projectFilePath,
                    new Dictionary<string, string>(),
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
