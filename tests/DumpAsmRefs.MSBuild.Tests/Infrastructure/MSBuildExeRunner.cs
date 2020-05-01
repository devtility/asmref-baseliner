// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using Microsoft.Build.Locator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class MSBuildExeRunner : AbstractExeBuildRunner
    {
        private string exePath;

        public MSBuildExeRunner(ITestOutputHelper output)
            :base(output)
        {
        }

        protected override string BinLogFileNamePrefix => "msbuild";

        protected override string ExePath { get { return exePath; } }

        protected override void InitializeBuild()
        {
            var queryOptions = new VisualStudioInstanceQueryOptions
            {
                DiscoveryTypes = DiscoveryType.VisualStudioSetup
            };
            var results = MSBuildLocator.QueryVisualStudioInstances(queryOptions).ToArray();

            WriteLine("Available MSBuild exes:");
            foreach (var item in results)
            {
                WriteLine($"  Version: {item.Version}, Path: {item.MSBuildPath}");
            }

            // If the NET Core build of the test assembly is being executed then no instances of VS will be found
            if (results.Length == 0)
            {
                WriteLine($"No instances of msbuild.exe found (are you running the NET Core version of the tests?");
                throw new NotSupportedException("No versions of msbuild.exe could be located");
            }

            exePath = System.IO.Path.Combine(results.Last().MSBuildPath, "msbuild.exe");
            WriteLine($"Using {exePath}");
        }

        protected override string BuildCommandLineArgs(string projectFilePath, string targetName,
            string binLogFilePath, Dictionary<string, string> additionalProperties)
        {
            var sb = new StringBuilder();
            sb.Append($"\"{projectFilePath}\"");
            sb.Append($" /t:{targetName} ");
            sb.Append($" /bl:\"{binLogFilePath}\"");
            sb.Append($" /nr:false");

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
    }
}
