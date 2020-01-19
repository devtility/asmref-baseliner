// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs
{
    public class FileSearchResult
    {
        public FileSearchResult(string baseDirectory, string[] includePatterns, string[] excludePatterns, string[] relativeFilePaths)
        {
            BaseDirectory = baseDirectory;
            IncludePatterns = includePatterns;
            ExcludePatterns = excludePatterns;
            RelativeFilePaths = relativeFilePaths;
        }

        public string BaseDirectory { get; }
        public string[] IncludePatterns { get; }
        public string[] ExcludePatterns { get; }
        public string[] RelativeFilePaths { get; }
    }
}
