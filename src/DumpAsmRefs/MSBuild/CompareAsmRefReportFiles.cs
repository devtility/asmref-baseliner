// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DumpAsmRefs
{
    public class CompareAsmRefReportFiles : Task
    {
        private readonly IFileSystem fileSystem;

        [Required]
        public string BaseLineReportFilePath { get; set; }

        [Required]
        public string CurrentReportFilePath { get; set; }

        [Required]
        public string VersionCompatibility { get; set; }

        public CompareAsmRefReportFiles()
        {
            fileSystem = new FileSystemAbstraction();
        }

        // Testing
        internal CompareAsmRefReportFiles(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public override bool Execute()
        {
            if (!TryGetVersionCompatibility(out var versionCompatibility)
                || !TryLoadReport(BaseLineReportFilePath, out var baseline)
                || !TryLoadReport(CurrentReportFilePath, out var current))
            {
                return false;
            }

            var comparer = new AsmRefResultComparer();
            bool result = comparer.AreSame(baseline, current, versionCompatibility);

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
            var loader = new YamlReportLoader();
            report = loader.Load(reportData);
            return true;
        }
    }
}
