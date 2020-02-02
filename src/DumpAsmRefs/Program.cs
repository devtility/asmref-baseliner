// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace DumpAsmRefs
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger(Verbosity.Diagnostic);
            var parser = new CommandLineParser();
            if (parser.TryParse(logger, args, out var userArguments))
            {
                logger.Verbosity = userArguments.Verbosity;
                Execute(userArguments, logger, new AssemblyFileLocator());
            }
        }

        internal static void Execute(UserArguments userArguments, ILogger logger, IFileLocator fileLocator)
        {
            logger.LogDebug(UIStrings.Matching_RootDirectory, userArguments.RootDirectory);
            var searchResult = fileLocator.Search(userArguments.RootDirectory, userArguments.IncludePatterns, userArguments.ExcludePatterns);

            if (searchResult.RelativeFilePaths.Count > 0)
            {
                logger.LogInfo(UIStrings.Matching_MatchesFound, searchResult.RelativeFilePaths.Count);
                DebugDumpList(UIStrings.Matching_ResultListHeader, searchResult.RelativeFilePaths, logger);

                var asmGenerator = new AssemblyInfoGenerator();
                var asmInfo = asmGenerator.Fetch(searchResult.BaseDirectory, searchResult.RelativeFilePaths);

                var reportBuilder = new TextFileReportBuilder();
                var data = reportBuilder.Generate(searchResult, asmInfo);

                File.WriteAllText(userArguments.OutputFileFullPath, data);
                logger.LogInfo(UIStrings.Program_ReportFileWritten, userArguments.OutputFileFullPath);
            }
            else
            {
                logger.LogWarning(UIStrings.Matching_NoFiles);
            }

            logger.LogDebug(UIStrings.Program_Finished);
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
