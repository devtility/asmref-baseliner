// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class DotNetBuildRunner : AbstractExeBuildRunner
    {
        private const string exePath = "dotnet";

        private static bool alreadyRun = false;
        
        public DotNetBuildRunner(ITestOutputHelper output)
            :base(output)
        {
        }

        protected override void InitializeBuild()
        {
            DumpDotNetVersion();

            if (!alreadyRun)
            {
                alreadyRun = true;
                DumpDotNetSdks();
            }
        }

        protected override string BinLogFileNamePrefix => "dotnet";

        protected override string ExePath => "dotnet";

        protected override string BuildCommandLineArgs(string projectFilePath, string targetName,
            string binLogFilePath, Dictionary<string, string> additionalProperties)
        {
            var sb = new StringBuilder();
            sb.Append($"build \"{projectFilePath}\"");
            sb.Append($" -t:{targetName} ");
            sb.Append($" -bl:LogFile=\"{binLogFilePath}\"");
            sb.Append($" -p:UseSharedCompilation=false"); // don't use the build server

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    sb.Append($" -p:{kvp.Key}=\"{kvp.Value}\"");
                }
            }

            var args = sb.ToString();
            return args;
        }

        private void DumpDotNetVersion()
        {
            var exeRunner = new ExeRunner(Logger);
            var executionResult = exeRunner.Run(exePath, "--version");
            Logger.WriteLine($"dotnet version: {executionResult.StandardOutput}");
        }

        private void DumpDotNetSdks()
        {
            var exeRunner = new ExeRunner(Logger);
            var executionResult = exeRunner.Run(exePath, "--list-sdks");
            Logger.WriteLine($"dotnet SDKs: {executionResult.StandardOutput}");

            Logger.WriteLine("");

            var locator = new DotNetSdkLocator(Logger);
            var sdks = locator.Find();
            foreach (var sdk in sdks)
            {
                Logger.WriteLine($"  {sdk.Path}    Exists: {Directory.Exists(sdk.Path)}    Parsed version: {sdk.Version}");
            }
        }
    }
}
