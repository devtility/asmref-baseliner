﻿// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

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
        public string BaselineReportFilePath { get; set; }

        [Required]
        public string CurrentReportFilePath { get; set; }

        [Required]
        public string SourceVersionCompatibility { get; set; }

        [Required]
        public bool IgnoreSourcePublicKeyToken { get; set; }

        [Required]
        public string TargetVersionCompatibility { get; set; }

        [Required]
        public bool RaiseErrorIfDifferent { get; set; }

        [Output]
        public bool ReportsAreSame { get; set; }

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
            Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_ComparisonSettingsHeaderText);
            if (!TryGetVersionCompatibility(SourceVersionCompatibility, nameof(SourceVersionCompatibility), out var sourceVersionCompatibility) ||
                !TryGetVersionCompatibility(TargetVersionCompatibility, nameof(TargetVersionCompatibility), out var targetVersionCompatibility))
            {
                return false;
            }
            Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_IgnoreSourcePublicKeyToken, IgnoreSourcePublicKeyToken);

            if (!TryLoadReport(BaselineReportFilePath, out var baseline)
                || !TryLoadReport(CurrentReportFilePath, out var current))
            {
                return false;
            }

            var options = new ComparisonOptions(sourceVersionCompatibility, IgnoreSourcePublicKeyToken, targetVersionCompatibility);

            ReportsAreSame = comparer.AreSame(baseline, current, options);

            if (ReportsAreSame)
            {
                this.Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_ReferencesAreSame,
                    BaselineReportFilePath, CurrentReportFilePath);
                return true;
            }
            else
            {
                if (RaiseErrorIfDifferent)
                {
                    this.Log.LogError(UIStrings.CompareTask_ReferencesAreDifferent,
                        BaselineReportFilePath, CurrentReportFilePath);
                    return false;
                }
                else
                {
                    this.Log.LogMessage(MessageImportance.High,
                            UIStrings.CompareTask_ReferencesAreDifferent,
                        BaselineReportFilePath, CurrentReportFilePath);
                    return true;
                }
            }
        }

        private bool TryGetVersionCompatibility(string inputValue, string inputPropertyName,  out VersionCompatibility versionCompatibility)
        {
            if (!System.Enum.TryParse(inputValue, ignoreCase: true, out versionCompatibility))
            {
                var allowedValues = string.Join(", ", System.Enum.GetNames(typeof(VersionCompatibility)));
                Log.LogError(UIStrings.CompareTask_InvalidVersionCompat, inputPropertyName, inputValue ?? "{null}", allowedValues);
                return false;
            }
            Log.LogMessage(MessageImportance.High, UIStrings.CompareTask_VersionCompat, inputPropertyName, versionCompatibility);
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
