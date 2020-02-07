// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DumpAsmRefs
{
    public class CompareAsmRefReportFiles : Task
    {
        private readonly IFileSystem fileSystem;

        public CompareAsmRefReportFiles()
        {
            fileSystem = new FileSystemAbstraction();
        }

        // Testing
        internal CompareAsmRefReportFiles(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        [Required]
        public string BaseLineReportFilePath { get; set; }

        [Required]
        public string CurrentReportFilePath { get; set; }

        public override bool Execute()
        {
            var comparer = new YamlReportComparer(fileSystem);

            bool result = comparer.AreSame(BaseLineReportFilePath, CurrentReportFilePath);

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
    }
}
