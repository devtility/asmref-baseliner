// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public class AssemblyFileLocator : IFileLocator
    {
        // TODO - handle cross-platform (case-sensitivity based on platform/mounted drive)
        private const StringComparison FileNameComparer = StringComparison.Ordinal;

        private static readonly string[] AssemblyFileExtensions = new[] { ".dll", ".exe" };

        // Factory method injection point for testing
        public delegate DirectoryInfoBase DirectoryWrapperFactoryMethod(string baseDirectory);
        private readonly DirectoryWrapperFactoryMethod directoryWrapperFactoryMethod;

        public AssemblyFileLocator()
            :this(CreateDirectoryWrapper)
        {
        }

        internal AssemblyFileLocator(DirectoryWrapperFactoryMethod factoryMethod)
        {
            this.directoryWrapperFactoryMethod = factoryMethod;
        }

        /// <summary>
        /// Factory method. Overridable for testing
        /// </summary>
        private static DirectoryInfoBase CreateDirectoryWrapper(string directoryPath)
        {
            return new DirectoryInfoWrapper(new System.IO.DirectoryInfo(directoryPath));
        }

        #region IFileLocator interfaces 

        public FileSearchResult Search(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var baseDirectoryInfo = directoryWrapperFactoryMethod(baseDirectory);
            var matchingPaths = SearchForFiles(baseDirectoryInfo, includePatterns, excludePatterns)
                .Where(IsAssembly)
                .ToArray();

            var results = new FileSearchResult(baseDirectory, includePatterns, excludePatterns, matchingPaths);
            return results;
        }

        #endregion

        private static IEnumerable<string> SearchForFiles(DirectoryInfoBase baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddIncludePatterns(includePatterns);
            matcher.AddExcludePatterns(excludePatterns ?? Array.Empty<string>());

            var directoryInfo = baseDirectory;
            var result = matcher.Execute(directoryInfo);

            return result.Files
                .Select(f => f.Path)
                .ToArray();
        }

        private static bool IsAssembly(string path)
        {
            var ext = System.IO.Path.GetExtension(path);
            return AssemblyFileExtensions.Any(afe => afe.Equals(ext, FileNameComparer));
        }
    }
}
