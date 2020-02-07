// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class CompareAsmRefReportFilesTests
    {
        [Fact]
        public void Compare_Same_NoError()
        {
            var dummyFileSystem = new FakeFileSystem();
            const string reportContent1 = @"#foo
123
456
# bar";

            const string reportContent2 = @"123
456";

            dummyFileSystem.AddFile("file1", reportContent1);
            dummyFileSystem.AddFile("file2", reportContent2);

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeTrue();
            buildEngine.ErrorEvents.Count.Should().Be(0);
            buildEngine.MessageEvents.Count.Should().Be(1);
            buildEngine.MessageEvents[0].Message.Contains("AAA");
            buildEngine.MessageEvents[0].Message.Contains("BBB");
        }

        [Fact]
        public void Compare_Different_Error()
        {
            var dummyFileSystem = new FakeFileSystem();
            dummyFileSystem.AddFile("file1", "AAA");
            dummyFileSystem.AddFile("file2", "BBB");

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeFalse();
            buildEngine.ErrorEvents.Count.Should().Be(1);
            buildEngine.ErrorEvents[0].Message.Contains("AAA");
            buildEngine.ErrorEvents[0].Message.Contains("BBB");
        }
    }
}
