// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class DotNetBuildRunner : AbstractExeBuildRunner
    {
        private const string exePath = "dotnet";

        public DotNetBuildRunner(ITestOutputHelper output)
            :base(output)
        {
        }

        protected override void InitializeBuild()
        {
            DumpDotNetVersion();
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
            var executionResult = ExeRunner.Run(exePath, "--version");
            WriteLine($"dotnet version: {executionResult.StandardOutput}");
        }
    }
}
