// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public class FileSearchResult
    {
        public FileSearchResult(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns, string[] relativeFilePaths)
        {
            BaseDirectory = baseDirectory;
            IncludePatterns = new List<string>(includePatterns ?? Enumerable.Empty<string>());
            ExcludePatterns = new List<string>(excludePatterns ?? Enumerable.Empty<string>());
            RelativeFilePaths = new List<string>(relativeFilePaths ?? Enumerable.Empty<string>());
        }

        public string BaseDirectory { get; }
        public IReadOnlyList<string> IncludePatterns { get; }
        public IReadOnlyList<string> ExcludePatterns { get; }
        public IReadOnlyList<string> RelativeFilePaths { get; }
    }
}
