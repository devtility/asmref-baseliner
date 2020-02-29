// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.IO;
using System.Linq;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class AssemblyFileLocatorTests
    {
        [Fact]
        public void SimpleFetch()
        {
            var mockDirectory = CreateInMemoryDirectory("c:\\root",
                    "aaa1.dll", "aaazzz.dll", "Xaaa1.dll", "Xzzzaaa1.dll", "aaa.notAnAssembly",
                    "sub1\\aaa2.dll", "sub1\\aaazzz.dll",
                    "sub1\\sub2\\aaa3.exe", "sub1\\sub2\\zzzaaa.dll");

            var testSubject = new AssemblyFileLocator(_ => mockDirectory);

            var includePatterns = new string[] { "**\\*aaa*"};
            var excludePatterns = new string[] { "**\\*zzz*" };

            var result = testSubject.Search("c:\\root\\", includePatterns, excludePatterns);

            // Checks
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(
                "aaa1.dll",
                "Xaaa1.dll",
                "sub1/aaa2.dll",
                "sub1/sub2/aaa3.exe");
        }

        private DirectoryInfoBase CreateInMemoryDirectory(string baseDir, params string[] relativePaths)
        {
            var mockDirectory = new InMemoryDirectoryInfo(baseDir,
                relativePaths.Select(r => Path.Combine(baseDir, r)));
            return mockDirectory;
        }
    }
}
