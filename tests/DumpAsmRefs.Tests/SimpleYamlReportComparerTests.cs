﻿// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class SimpleYamlReportComparerTests
    {
        [Fact]
        public void MissingFiles_Throws()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists("existingFile")).Returns(true);
            fileSystem.Setup(x => x.FileExists("missingFile")).Returns(false);

            var testSubject = new SimpleYamlReportComparer(fileSystem.Object);

            // Missing baseline
            Action act = () => testSubject.AreSame("missingFile", "existingFile");
            act.Should().ThrowExactly<FileNotFoundException>("missingFile");

            // Missing comparison file
            act = () => testSubject.AreSame("existingFile", "missingFile");
            act.Should().ThrowExactly<FileNotFoundException>("missingFile");
        }

        [Fact]
        public void ReportsDifferByComment_AreSame_IsTrue()
        {
            const string contents1 = @"# 1111
aaa
# 222
bbb ccc
# 333
";

            const string contents2 = @"# XXXX
aaa
# XXX
bbb ccc
# XXX


# Blank lines above should be ignored
";
            var dummyFS = new FakeFileSystem();
            dummyFS.AddFile("file1", contents1);
            dummyFS.AddFile("file2", contents2);

            var testSubject = new SimpleYamlReportComparer(dummyFS);
            testSubject.AreSame("file1", "file1").Should().BeTrue();
            testSubject.AreSame("file1", "file2").Should().BeTrue();
        }

        [Fact]
        public void ReportsDiffer_AreSame_IsTrue()
        {
            const string contents1 = @"# 1111
aaa
";

            const string contents2 = @"# XXXX
AAA";
            var dummyFS = new FakeFileSystem();
            dummyFS.AddFile("file1", contents1);
            dummyFS.AddFile("file2", contents2);

            var testSubject = new SimpleYamlReportComparer(dummyFS);
            testSubject.AreSame("file2", "file2").Should().BeTrue();
            testSubject.AreSame("file1", "file2").Should().BeFalse();
        }

        [Fact]
        public void LineProcessing()
        {
            // Text, no comments
            SimpleYamlReportComparer.GetProcessedLine("12 3").Should().Be("12 3");
            SimpleYamlReportComparer.GetProcessedLine("AAAbbbb   ").Should().Be("AAAbbbb");

            // Whitespace
            SimpleYamlReportComparer.GetProcessedLine(null).Should().BeNull();
            SimpleYamlReportComparer.GetProcessedLine("").Should().BeNull();
            SimpleYamlReportComparer.GetProcessedLine("\t \n").Should().BeNull();

            // Whitespace with comment
            SimpleYamlReportComparer.GetProcessedLine("  # comment").Should().BeNull();
            SimpleYamlReportComparer.GetProcessedLine("\t# comment  ").Should().BeNull();

            // Text with comment
            SimpleYamlReportComparer.GetProcessedLine(" 123  # comment").Should().Be(" 123");
        }
    }
}