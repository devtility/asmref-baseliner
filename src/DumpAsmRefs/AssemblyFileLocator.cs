// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DumpAsmRefs
{
    public class AssemblyFileLocator : IFileLocator
    {
        // TODO - handle cross-platform (case-sensitivity based on platform/mounted drive)
        private const StringComparison FileNameComparer = StringComparison.Ordinal;

        private static readonly string[] AssemblyFileExtensions = new[] { ".dll", ".exe" };

        #region IFileLocator interfaces 

        public FileSearchResult Search(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var matchingPaths = SearchForFiles(baseDirectory, includePatterns, excludePatterns)
                .Where(IsAssembly)
                .ToArray();

            var results = new FileSearchResult(baseDirectory, includePatterns, excludePatterns, matchingPaths);
            return results;
        }

        #endregion

        private static IEnumerable<string> SearchForFiles(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddIncludePatterns(includePatterns);
            matcher.AddExcludePatterns(excludePatterns ?? Array.Empty<string>());

            var directoryInfo = new DirectoryInfoWrapper(new DirectoryInfo(baseDirectory));
            var result = matcher.Execute(directoryInfo);

            return result.Files
                .Select(f => f.Path)
                .ToArray();
        }

        private static bool IsAssembly(string path)
        {
            var ext = Path.GetExtension(path);
            return AssemblyFileExtensions.Any(afe => afe.Equals(ext, FileNameComparer));
        }
    }
}
