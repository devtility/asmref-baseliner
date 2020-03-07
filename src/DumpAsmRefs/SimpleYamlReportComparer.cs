// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;

namespace DumpAsmRefs
{
    /// <summary>
    /// Simple YAML comparer.
    /// </summary>
    /// <remarks>
    /// Ignores comments but otherwise expects the text to be identical
    /// </remarks>
    internal class SimpleYamlReportComparer
    {
        private readonly IFileSystem fileSystem;

        public SimpleYamlReportComparer()
            : this(new FileSystemAbstraction())
        {
        }

        internal SimpleYamlReportComparer(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public bool AreSame(string baselineFilePath, string comparisonFilePath)
        {
            if (!fileSystem.FileExists(baselineFilePath))
            {
                throw new System.IO.FileNotFoundException(UIStrings.ReportComparer_Error_ReportNotFound, baselineFilePath);
            }

            if (!fileSystem.FileExists(comparisonFilePath))
            {
                throw new System.IO.FileNotFoundException(UIStrings.ReportComparer_Error_ReportNotFound, comparisonFilePath);
            }

            var source = GetProcessedLines(baselineFilePath);
            var target = GetProcessedLines(comparisonFilePath);

            if (source.Length != target.Length)
            {
                return false;
            }

            for (int index = 0; index < source.Length; index++)
            {
                if (!source[index].Equals(target[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private string[] GetProcessedLines(string filePath)
        {
            var originalLines = fileSystem.ReadAllLines(filePath);
            var processed = new List<string>();

            foreach(var line in originalLines)
            {
                var processedLine = GetProcessedLine(line);
                if (processedLine != null)
                {
                    processed.Add(processedLine);
                }
            }

            return processed.ToArray();
        }

        internal static string GetProcessedLine(string line)
        {
            // Strips comments and trailing whitespace
            // Note: does not correctly handle comment markers in whitespace
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var processedLine = line;
            var startCommentIndex = line.IndexOf('#');
            if (startCommentIndex > -1)
            {
                processedLine = line.Substring(0, startCommentIndex);
            }

            processedLine = processedLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(processedLine))
            {
                return null;
            }

            return processedLine;
        }
    }
}
