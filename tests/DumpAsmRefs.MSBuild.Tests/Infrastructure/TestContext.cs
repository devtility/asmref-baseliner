// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.IO;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class TestContext
    {
        // Fields to control one-time static initialisation.
        // The test NuGet package should be deleted from the test package cache
        // when the first TestContext is created.
        private static bool initialized;
        private static readonly object lockObject = new object();

        private const string PackageName = "Devtility.CheckAsmRefs";

        private readonly ITestOutputHelper output;

        internal static TestContext Initialize(ITestOutputHelper output,
            [System.Runtime.CompilerServices.CallerMemberName] string uniqueTestName = "") => new TestContext(output, uniqueTestName);

        private TestContext(ITestOutputHelper output,
            [System.Runtime.CompilerServices.CallerMemberName] string uniqueTestName = "")
        {
            this.output = output;
            output.WriteLine($"Initializing context for test '{uniqueTestName}'");

            TestResultsDirectory = GetTestResultsPath();
            TestSpecificDirectory = CreateTestSpecificDirectory(uniqueTestName);

            var (packageFilePath, version) = GetLatestNuGetPackagePathAndVersion();
            LocalNuGetFeedPath = Path.GetDirectoryName(packageFilePath);
            PackageVersion = version;

            NuGetPackageCachePath = Path.Combine(GetTestResultsPath(), "TestPackagesCache");
            EnsureLocalNuGetConfigFileExists(LocalNuGetFeedPath);
            EnsureTestNuGetPackageDeletedOnFirstRun(NuGetPackageCachePath);
        }

        public string TestResultsDirectory { get; }
        public string TestSpecificDirectory { get; }
        public string PackageVersion { get; }
        public string NuGetPackageCachePath { get; }

        private string LocalNuGetFeedPath { get; }

        private static string GetTestResultsPath()
        {
            const string folderName = "\\DumpAsmRefs.MSBuild.Tests\\";
            var projectBinPath = GetTestAssemblyBinPath();

            var index = projectBinPath.IndexOf(folderName);
            var projectDirectory = projectBinPath.Substring(0, index);
            return Path.Combine(projectDirectory, "TestResults");
        }

        private string CreateTestSpecificDirectory(string subDirName)
        {
            var directory = Path.Combine(TestResultsDirectory, subDirName);
            output.WriteLine($"Test-specific directory: {directory}");

            SafeDeleteDirectory(directory);

            Directory.CreateDirectory(directory);
            return directory;
        }

        private void EnsureLocalNuGetConfigFileExists(string feedPath)
        {
            // Ensure there is a local NuGet config file in the test results folder
            // that will be picked up by any tests in that folder.
            // The file configures the feeds to use so that the locally-built NuGet package
            // is picked up, rather than a published version.

            var fullPath = Path.Combine(GetTestResultsPath(), "nuget.config");
            if (File.Exists(fullPath))
            {
                output.WriteLine($"Local nuget.config file already exists: {fullPath}");
                return;
            }

            var nugetConfigContent = $@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <packageSources>
        <clear /> <!-- ensure only the sources defined below are used -->
        <add key='latestPackageFolder' value='{feedPath}' />
        <add key='NuGet official package source' value='https://api.nuget.org/v3/index.json' />
    </packageSources>
</configuration>
";
            output.WriteLine($"Creating local nuget.config file: {fullPath}");
            output.WriteLine(nugetConfigContent);
            File.WriteAllText(fullPath, nugetConfigContent);
        }

        private void EnsureTestNuGetPackageDeletedOnFirstRun(string nuGetPackageCachePath)
        {
            lock (lockObject)
            {
                output.WriteLine("Test NuGet package was deleted by a previous test.");
                if (initialized)
                {
                    return;
                }

                var path = Path.Combine(nuGetPackageCachePath, PackageName);
                output.WriteLine($"Deleting test NuGet package directory: {path}");
                SafeDeleteDirectory(path);

                initialized = true;
            }
        }

        private static string GetTestAssemblyBinPath()
        {
            var uriCodeBase = typeof(DotNetBuildTests).Assembly.CodeBase;
            var uri = new Uri(uriCodeBase);
            var path = uri.AbsolutePath;
            return Path.GetDirectoryName(path);
        }

        private (string packageFilePath, string version) GetLatestNuGetPackagePathAndVersion()
        {
            const string filePrefix = PackageName + ".";
            var directory = GetTestAssemblyBinPath();
            var files = Directory.GetFiles(directory, $"{filePrefix}*.nupkg");

            if (files.Length != 1)
            {
                throw new InvalidOperationException("Test setup error: failed to locate the current NuGet package");
            }

            var version = Path.GetFileNameWithoutExtension(files[0]).Replace(filePrefix, "");
            output.WriteLine($"Latest NuGet package path: {files[0]}");
            output.WriteLine($"Latest NuGet package version: {version}");
            return (files[0], version);
        }

        public string WriteFile(string fileName, string text, string subdir = "")
        {
            var fullDirPath = Path.Combine(TestSpecificDirectory, subdir);
            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            var fullPathName = Path.Combine(fullDirPath, fileName);
            output.WriteLine($"Creating test-specific file: {fullPathName}");
            File.WriteAllText(fullPathName, text);
            return fullPathName;
        }

        private void SafeDeleteDirectory(string directory)
        {
            var attempts = 0;
            while (attempts < 3 && Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    output.WriteLine($"Test setup error cleaning directory '{directory}'. {ex}");
                }
                attempts++;
            }
            output.WriteLine($"Directory deleted: {directory}");
        }
    }
}
