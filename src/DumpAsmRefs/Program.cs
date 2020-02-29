// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public enum ExitCodes
    {
        Success = 0,
        ParsingError = 1,
        NoMatchingFiles = 2
    }

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("AsmRef_LaunchDebugger")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                System.Diagnostics.Debugger.Launch();
            }

            var logger = new ConsoleLogger(Verbosity.Diagnostic);
            var parser = new CommandLineParser();
            if (parser.TryParse(logger, args, out var userArguments))
            {
                logger.Verbosity = userArguments.Verbosity;
                return Execute(userArguments, new AssemblyFileLocator(),
                    new AssemblyInfoGenerator(), new YamlReportBuilder(), 
                    new FileSystemAbstraction(), logger);
            }

            return (int)ExitCodes.ParsingError;
        }

        internal static int Execute(UserArguments userArguments, IFileLocator fileLocator,
            IAssemblyInfoGenerator assemblyInfoGenerator, IReportBuilder reportBuilder, IFileSystem fileSystem, ILogger logger)
        {
            logger.Verbosity = userArguments.Verbosity;
            logger.LogDebug(UIStrings.Matching_RootDirectory, userArguments.RootDirectory);
            var matchingPaths = fileLocator.Search(userArguments.RootDirectory, userArguments.IncludePatterns, userArguments.ExcludePatterns);

            if (!matchingPaths.Any())
            {
                logger.LogWarning(UIStrings.Matching_NoFiles);
                return (int)ExitCodes.NoMatchingFiles;
            }

            var inputs = new InputCriteria(userArguments.RootDirectory, userArguments.IncludePatterns,
                userArguments.ExcludePatterns, matchingPaths);

            logger.LogInfo(UIStrings.Matching_MatchesFound, inputs.RelativeFilePaths.Count);
            DebugDumpList(UIStrings.Matching_ResultListHeader, inputs.RelativeFilePaths, logger);

            var asmInfo = assemblyInfoGenerator.Fetch(inputs.BaseDirectory, inputs.RelativeFilePaths);

            var result = new AsmRefResult(inputs, asmInfo);
            var data = reportBuilder.Generate(result);

            fileSystem.WriteAllText(userArguments.OutputFileFullPath, data);
            logger.LogInfo(UIStrings.Program_ReportFileWritten, userArguments.OutputFileFullPath);
 
            logger.LogDebug(UIStrings.Program_Finished);
            return (int)ExitCodes.Success;
        }

        private static void DebugDumpList(string headerMessage, IEnumerable<string> items, ILogger logger)
        {
            logger.LogDebug(headerMessage);
            int itemNumber = 1;
            foreach(var item in items)
            {
                var prefix = itemNumber.ToString().PadLeft(3);
                logger.LogDebug(UIStrings.Matching_ResultListItem, prefix, item);
                itemNumber++;
            }
        }
    }
}
