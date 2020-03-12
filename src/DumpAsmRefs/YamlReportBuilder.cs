// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DumpAsmRefs
{
    internal class YamlReportBuilder : IReportBuilder
    {
        public const string DocumentSeparator = "---";
        public const string StreamTerminator = "...";

        private StringBuilder sb;

        #region IReportBuilder implementation

        public string Generate(AsmRefResult asmRefResult)
        {
            sb = new StringBuilder();

            WriteHeader(asmRefResult.InputCriteria);

            foreach (var item in asmRefResult.AssemblyReferenceInfos.OrderBy(x => x.SourceAssemblyRelativePath))
            {
                WriteSingleAssemblyInfo(item);
            }

            WriteStreamEnd();
            return sb.ToString();
        }

        #endregion

        private void WriteHeader(InputCriteria inputCriteria)
        {
            WriteDocumentStart();

            WriteBoilerplateHeader();

            WriteComment("Base directory: " + inputCriteria.BaseDirectory);
            if (inputCriteria.IncludePatterns != null)
            {
                WriteList("Include patterns", inputCriteria.IncludePatterns.OrderBy(x => x));
            }
            if (inputCriteria.ExcludePatterns != null)
            {
                WriteList("Exclude patterns", inputCriteria.ExcludePatterns.OrderBy(x => x));
            }

            WriteComment("Number of matches: " + inputCriteria.RelativeFilePaths.Count().ToString());
            WriteSpacer();
        }

        private void WriteBoilerplateHeader()
        {
            var versionAtt = this.GetType().Assembly
                .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute;

            var versionText = versionAtt?.Version.ToString() ?? "{unknown}";

            var header = string.Format(UIStrings.Report_HeaderInstructions,
                DateTime.UtcNow.ToString("o"),
                versionText
                );
            sb.AppendLine(header);
        }

        private void WriteSingleAssemblyInfo(AssemblyReferenceInfo asmRefInfo)
        {
            WriteDocumentStart();

            WriteProperty("Assembly", asmRefInfo.SourceAssemblyName?.ToString() ?? "{unknown}");
            WriteProperty("Relative path", asmRefInfo.SourceAssemblyRelativePath);
            WriteSpacer();

            if (asmRefInfo.LoadException != null)
            {
                WriteProperty("Assembly load exception", asmRefInfo.LoadException);
                WriteSpacer();
            }

            if (asmRefInfo.ReferencedAssemblies != null)
            {
                WriteList("Referenced assemblies", asmRefInfo.ReferencedAssemblies);
                WriteComment($"Number of references: {asmRefInfo.ReferencedAssemblies.Count()}");
            }

            WriteSpacer();
        }

        private void WriteDocumentStart()
        {
            sb.AppendLine(DocumentSeparator);
        }

        private void WriteStreamEnd()
            => sb.AppendLine(StreamTerminator);

        private void WriteComment(string text)
            => sb.AppendLine("# " + text);

        private void WriteSpacer()
            => sb.AppendLine();

        private void WriteProperty(string propertyName, string value)
            => sb.Append(propertyName).Append(": ").AppendLine(Escape(value));

        private void WriteList(string propertyName, IEnumerable<string> values)
        {
            sb.Append(propertyName).AppendLine(":");
            foreach (var item in values?.OrderBy(x => x))
            {
                sb.Append("- ").AppendLine(Escape(item));
            }
        }

        private static string Escape(string value)
            => $"'{value}'";
    }
}
