// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class DotNetSdkLocator
    {
        private readonly ITestOutputHelper outputHelper;

        public DotNetSdkLocator(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public IEnumerable<DotNetSdk> Find()
        {
            var executionResult = ExeRunner.Run("dotnet", "--list-sdks");
            WriteLine($"dotnet SDKs: {executionResult.StandardOutput}");

            // e.g 2.1.202 [C:\Program Files\dotnet\sdk]
            var listSdksOutput = executionResult.StandardOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var sdks = listSdksOutput
                .Select(ToDotNetSdks)
                .Where(s => s != null)
                .ToArray();

            return sdks;
        }

        private static DotNetSdk ToDotNetSdks(string listSdksItem)
        {
            // e.g. 2.1.202 [C:\Program Files\dotnet\sdk]
            var match = Regex.Match(listSdksItem, @"^(.+) \[(.+)\]");

            if (match.Success)
            {
                var path = match.Groups[2].Value;
                var ver = match.Groups[1].Value;

                var fullPath = Path.Combine(path, ver);
                return new DotNetSdk(ver, fullPath);
            }

            return null;
        }

        protected void WriteLine(string message)
        {
            outputHelper.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
