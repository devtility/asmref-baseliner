// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.Linq;
using System.Text;

namespace DumpAsmRefs
{
    internal class YamlReportBuilder : IReportBuilder
    {
        #region IReportBuilder implementation

        public string Generate(AsmRefResult asmRefResult)
        {
            var sb = new StringBuilder();

            WriteHeader(sb, asmRefResult.InputCriteria);

            foreach (var item in asmRefResult.AssemblyReferenceInfos.OrderBy(x => x.SourceAssemblyRelativePath))
            {
                WriteSingleAssemblyInfo(sb, item);
            }

            sb.AppendLine("...");
            return sb.ToString();
        }

        #endregion

        private static readonly string[] NotItemsStringArray = new string[] { "none" } ;

        private static void WriteHeader(StringBuilder sb, InputCriteria fileSearchResult)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.Append("# Base directory: ").AppendLine(fileSearchResult.BaseDirectory);
            sb.Append("# Include patterns: ").AppendLine(string.Join(", ", fileSearchResult.IncludePatterns ?? NotItemsStringArray));
            sb.Append("# Exclude patterns: ").AppendLine(string.Join(", ", fileSearchResult.ExcludePatterns ?? NotItemsStringArray));
            sb.Append("# Number of matches: ").AppendLine(fileSearchResult.RelativeFilePaths.Count().ToString());
            sb.AppendLine();
        }

        private static void WriteSingleAssemblyInfo(StringBuilder sb, AssemblyReferenceInfo asmRefInfo)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.Append("Assembly: ").AppendLine(asmRefInfo.SourceAssemblyName?.ToString() ?? "{unknown}");
            sb.Append("Relative path: ").AppendLine(asmRefInfo.SourceAssemblyRelativePath);
            sb.AppendLine();
            sb.Append("Referenced assemblies:   # count = ")
                .AppendLine(asmRefInfo.ReferencedAssemblies?.Count().ToString() ?? "{unknown}");

            if (asmRefInfo.LoadException != null)
            {
                sb.Append($"Assembly load exception: {asmRefInfo.LoadException}");
            }

            if (asmRefInfo.ReferencedAssemblies != null)
            {
                foreach (var refdItem in asmRefInfo.ReferencedAssemblies?.OrderBy(x => x))
                {
                    sb.Append("- ").AppendLine(refdItem);
                }
            }

            sb.AppendLine();
        }
    }
}
