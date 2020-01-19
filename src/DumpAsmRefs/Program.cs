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
            Execute(args, Directory.GetCurrentDirectory(),
                new ConsoleLogger(Verbosity.Diagnostic), new AssemblyFileLocator());
        }

        internal static void Execute(string[] args, string baseDirectory, ILogger logger, IFileLocator fileLocator)
        {
            logger.LogDebug(UIStrings.Matching_BaseDirectory, baseDirectory);
            var searchResult = fileLocator.Search(baseDirectory, args, null);

            if (searchResult.RelativeFilePaths.Length > 0)
            {
                logger.LogInfo(UIStrings.Matching_MatchesFound, searchResult.RelativeFilePaths.Length);
                DebugDumpList(UIStrings.Matching_ResultListHeader, searchResult.RelativeFilePaths, logger);

                var asmGenerator = new AssemblyInfoGenerator();
                var asmInfo = asmGenerator.Fetch(searchResult);

                var reportBuilder = new TextFileReportBuilder();
                var data = reportBuilder.Generate(searchResult, asmInfo);

                var reportPath = Path.Combine(baseDirectory, "ReferencedAssemblies.txt");
                File.WriteAllText(reportPath, data);
                logger.LogInfo(UIStrings.Program_ReportFileWritten, reportPath);
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
