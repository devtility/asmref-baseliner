// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.Collections.Generic;

namespace DumpAsmRefs
{
    internal class UserArguments
    {
        public UserArguments(string rootDirectory, IEnumerable<string> includePatterns,
            IEnumerable<string> excludePatterns, string outputFileName, Verbosity verbosity)
        {
            RootDirectory = rootDirectory;
            IncludePatterns = includePatterns;
            ExcludePatterns = excludePatterns;
            Verbosity = verbosity;
            OutputFileFullPath = outputFileName;
        }

        public string RootDirectory { get; }

        public IEnumerable<string> IncludePatterns { get; }
        public IEnumerable<string> ExcludePatterns { get; }

        public Verbosity Verbosity { get; }
        public string OutputFileFullPath { get; }
    }
}
