using FluentAssertions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AsmRefBaseliner.MSBuild.Tests
{
    public class DotNetBuildTests
    {
        public DotNetBuildTests()
        {
            // Must be done in a separate method, before any code that uses the
            // Microsoft.Build namespace.
            // See https://github.com/microsoft/MSBuildLocator/commit/f3d5b0814bc7c5734d03a617c17c6998dd2f0e99
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
        }

        [Fact]
        public void SimpleBuild()
        {
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

            var p = new BuildParameters
            {
                Loggers = new ILogger[] { new Microsoft.Build.Logging.BinaryLogger { Parameters = "proj1\\msbuild.binlog" }  }
            };

            var buildResult = BuildManager.DefaultBuildManager.Build(p, 
                new BuildRequestData(projFilePath,
                    new Dictionary<string, string>(),
                    null,
                    new[] { "Restore", "Build" },
                    null,
                    BuildRequestDataFlags.None));

            buildResult.OverallResult.Should().Be(BuildResultCode.Success);

            var binLogFilPath = Path.Combine(Environment.CurrentDirectory, "proj1", "msbuild.binlog");
            File.Exists(binLogFilPath).Should().BeTrue();
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
    }
}
