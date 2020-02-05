// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DumpAsmRefs
{
    public class CompareReportFiles : Task
    {
        private readonly IFileSystem fileSystem;

        public CompareReportFiles() : this(new FileSystemAbstraction())
        {
        }

        internal CompareReportFiles(IFileSystem fileSystem)
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
                this.Log.LogMessage(MessageImportance.High, "Files are the same. Source: {0}, Target: {1}",
                    BaseLineReportFilePath, CurrentReportFilePath);
                return true;
            }
            else
            {
                this.Log.LogError("Files are different. Source: {0}, Target: {1}",
                    BaseLineReportFilePath, CurrentReportFilePath);
                return false;
            }
        }
    }
}
