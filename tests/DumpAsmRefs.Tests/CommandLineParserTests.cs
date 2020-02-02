// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using System.IO;
using System.Linq;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class CommandLineParserTests
    {
        [Fact]
        public void AllValidArgs_Succeeds()
        {
            var testSubject = new CommandLineParser();
            var logger = new TestLogger();

            var result = testSubject.TryParse(logger,
                new string[] {
                    "-r:d:\\current", "-v:detailed", "-o:myfile.yml", "include1*", "!Exclude1*", "!Exclude2*", "include2*"
                },
                out var userArguments);

            result.Should().BeTrue();

            userArguments.Should().NotBeNull();
            userArguments.RootDirectory.Should().Be("d:\\current");
            userArguments.OutputFileFullPath.Should().Be("d:\\current\\myfile.yml");
            userArguments.IncludePatterns.Should().BeEquivalentTo("include1*", "include2*");
            userArguments.ExcludePatterns.Should().BeEquivalentTo("Exclude1*", "Exclude2*");
            userArguments.Verbosity.Should().Be(Verbosity.Detailed);

            logger.Errors.Should().BeEmpty();
        }

        [Fact]
        public void MinimumValidArgs_Succeeds()
        {
            var testSubject = new CommandLineParser();
            var logger = new TestLogger();

            var result = testSubject.TryParse(logger,
                new string[] {
                    "**/include*"
                },
                out var userArguments);

            result.Should().BeTrue();

            var expectedDir = Directory.GetCurrentDirectory();
            var expectedFilePath = Path.Combine(expectedDir, "ReferencedAssemblies.yml");

            userArguments.Should().NotBeNull();
            userArguments.RootDirectory.Should().Be(expectedDir);
            userArguments.OutputFileFullPath.Should().Be(expectedFilePath);
            userArguments.IncludePatterns.Should().BeEquivalentTo("**/include*");
            userArguments.ExcludePatterns.Should().BeEmpty();
            userArguments.Verbosity.Should().Be(Verbosity.Normal);

            logger.Errors.Should().BeEmpty();
        }

        [Fact]
        public void NoArgs_Fails()
        {
            var testSubject = new CommandLineParser();
            var logger = new TestLogger();

            var result = testSubject.TryParse(logger,
                new string[] { },
                out var userArguments);

            result.Should().BeFalse();
            userArguments.Should().BeNull();
            logger.Errors.Should().Contain(UIStrings.Parser_IncludePatternRequired);
        }

        [Fact]
        public void InvalidVerbosity_Fails()
        {
            var testSubject = new CommandLineParser();
            var logger = new TestLogger();

            var result = testSubject.TryParse(logger,
                new string[] { "--verbosity:xxx", "include*"},
                out var userArguments);

            result.Should().BeFalse();
            userArguments.Should().BeNull();
            logger.Errors.Any(m => m.Contains(UIStrings.Parser_Error_InvalidVerbosity));
        }
    }
}
