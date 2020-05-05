// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class TargetsInputs
    {
        public string AsmRefIncludePatterns { get; set; }
        public string AsmRefRootSearchDir { get; set; }
        public string AsmRefOutputFilePath { get; set; }
        public string AsmRefBaselineFilePath { get; set; }
        public string AsmRefLogLevel { get; set; }
        public string AsmRefVersionCompatibility { get; set; }
        public string AsmRefSourceVersionCompatibility { get; set; }
        public string AsmRefTargetVersionCompatibility  { get; set; }
    }
}
