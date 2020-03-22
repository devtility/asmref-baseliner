// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DumpAsmRefs
{
    public class CompareAsmRefReportFiles : Task
    {
        private readonly IFileSystem fileSystem;
        private readonly IReportLoader loader;
        private readonly IResultComparer comparer;

        [Required]
        public string BaseLineReportFilePath { get; set; }

        [Required]
        public string CurrentReportFilePath { get; set; }

        [Required]
        public string VersionCompatibility { get; set; }

        public CompareAsmRefReportFiles()
            : this(new FileSystemAbstraction(), new YamlReportLoader(),  new AsmRefResultComparer()) { }

        // Testing
        internal CompareAsmRefReportFiles(IFileSystem fileSystem, IReportLoader loader, IResultComparer comparer)
        {
            this.fileSystem = fileSystem;
            this.loader = loader;
            this.comparer = comparer;
        }

        public override bool Execute()
        {
            if (!TryGetVersionCompatibility(out var versionCompatibility)
                || !TryLoadReport(BaseLineReportFilePath, out var baseline)
                || !TryLoadReport(CurrentReportFilePath, out var current))
            {
                return false;
            }

            var options = new ComparisonOptions(versionCompatibility);

            bool result = comparer.AreSame(baseline, current, options);

            if (result)
            {
                this.Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_ReferencesAreSame,
                    BaseLineReportFilePath, CurrentReportFilePath);
                return true;
            }
            else
            {
                this.Log.LogError(UIStrings.CompareTask_ReferencesAreDifferent,
                    BaseLineReportFilePath, CurrentReportFilePath);
                return false;
            }
        }

        private bool TryGetVersionCompatibility(out VersionCompatibility versionCompatibility)
        {
            if (!System.Enum.TryParse(VersionCompatibility, ignoreCase: true, out versionCompatibility))
            {
                var allowedValues = string.Join(", ", System.Enum.GetNames(typeof(VersionCompatibility)));
                Log.LogError(UIStrings.CompareTask_InvalidVersionCompat, VersionCompatibility ?? "{null}", allowedValues);
                return false;
            }
            Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_VersionCompat, versionCompatibility);
            return true;
        }

        private bool TryLoadReport(string filePath, out AsmRefResult report)
        {
            if (!fileSystem.FileExists(filePath))
            {
                Log.LogError(UIStrings.CompareTask_Error_ReportNotFound, filePath);
                report = null;
                return false;
            }

            var reportData = fileSystem.ReadAllText(filePath);
            report = loader.Load(reportData);
            return true;
        }
    }
}
