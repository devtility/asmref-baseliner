// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace DumpAsmRefs.Tests
{
    public class YamlReportBuilderTests
    {
        private readonly ITestOutputHelper output;

        public YamlReportBuilderTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ReportHeader_ContainsSearchData()
        {
            var inputs = new InputCriteria("BASE DIR",
                new string[] { "include2", "include1" },
                new string[] { "exclude1", "exclude2" },
                new string[] { "path1", "path2"});

            var report = new AsmRefResult(inputs, Enumerable.Empty<AssemblyReferenceInfo>());
            var testSubject = new YamlReportBuilder();

            var result = testSubject.Generate(report);

            output.WriteLine(result);

            // Check report contents
            result.StartsWith(YamlReportBuilder.DocumentSeparator).Should().BeTrue();

            // Check boilerplate header content
            var expectedVersion = testSubject.GetType().Assembly
                .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).First() as AssemblyFileVersionAttribute;
            result.Should().Contain("v" + expectedVersion.Version.ToString());
            result.Should().Contain("https://github.com/devtility/asmref-baseliner/");

            // Check search criteria
            result.Should().Contain("# Base directory: BASE DIR");
            result.Should().Contain("- 'include1'");
            result.Should().Contain("- 'include2'");
            result.Should().Contain("- 'exclude1'");
            result.Should().Contain("- 'exclude2'");
            result.Should().Contain("2"); // number of matches - specific paths are not listed in the header

            // Check ordering
            result.IndexOf("- 'include1'").Should().BeLessThan(result.IndexOf("- 'include2'"));

            CheckExpectedNumberOfDocs(result, 1);
            CheckStartsWithDocSeparator(result);
            CheckEndsWithStreamTermiator(result);
        }

        [Fact]
        public void ReportBody_ContainsAsmData()
        {
            var inputs = new InputCriteria("BASE DIR", new string[] { "include1" },
                new string[] { "exclude1"}, new string[] { "path1" });

            var asmRefInfos = new AssemblyReferenceInfo[]
            {
                new AssemblyReferenceInfo()
                {
                    LoadException = null,
                    SourceAssemblyFullPath = "full path1",
                    SourceAssemblyName = "asmName1",
                    SourceAssemblyRelativePath = "relative path1",
                    ReferencedAssemblies = new string[]{ "asm 1_1", "asm 1_2" }
                },
                new AssemblyReferenceInfo()
                {
                    LoadException = null,
                    SourceAssemblyFullPath = "full path2",
                    SourceAssemblyName = "asmName2",
                    SourceAssemblyRelativePath = "relative path2",
                    ReferencedAssemblies = new string[] { "asm 2_1", "asm 2_2" }
                },
                new AssemblyReferenceInfo()
                {
                    LoadException = "image format exception",
                    SourceAssemblyFullPath = "full path3",
                    SourceAssemblyName = "asmName3",
                    SourceAssemblyRelativePath = "relative path3",
                    ReferencedAssemblies = null
                }
            };

            var report = new AsmRefResult(inputs, asmRefInfos);

            var testSubject = new YamlReportBuilder();

            var result = testSubject.Generate(report);

            output.WriteLine(result);

            // Check report contents
            result.Should().Contain("asmName1");
            result.Should().Contain("relative path1");
            result.Should().Contain("asm 1_1");
            result.Should().Contain("asm 1_2");

            result.Should().Contain("asmName2");
            result.Should().Contain("relative path2");
            result.Should().Contain("asm 2_1");
            result.Should().Contain("asm 2_2");

            result.Should().Contain("asmName3");
            result.Should().Contain("relative path3");
            result.Should().Contain("image format exception");

            // Not expecting fully-qualified names as that would give different results
            // when built on different machines
            result.Should().NotContain("full path1");
            result.Should().NotContain("full path2");
            result.Should().NotContain("full path3");

            // Cound the number of documents:
            // Expected = 1 for the header, + 1 per assembly
            CheckExpectedNumberOfDocs(result, 4);

            CheckStartsWithDocSeparator(result);
            CheckEndsWithStreamTermiator(result);
        }

        private static void CheckExpectedNumberOfDocs(string report, int expected)
            => Regex.Matches(report, "^---\r?$", RegexOptions.Multiline)
                .Count.Should().Be(expected);

        private static void CheckStartsWithDocSeparator(string report)
            => report.StartsWith(YamlReportBuilder.DocumentSeparator).Should().BeTrue();

        private static void CheckEndsWithStreamTermiator(string report)
            => report.TrimEnd().EndsWith(YamlReportBuilder.StreamTerminator).Should().BeTrue();
    }
}
