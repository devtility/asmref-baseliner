// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class ComparisonTaskInputs
    {
        public string BaselineReportFilePath { get; set; }
        public string CurrentReportFilePath { get; set; }
        public string SourceVersionCompatibility { get; set; }
        public string TargetVersionCompatibility { get; set; }
        public string IgnoreSourcePublicKeyToken { get; set; }
    }
}
