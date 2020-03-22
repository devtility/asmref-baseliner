// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class CompareAsmRefReportFilesTests
    {
        [Fact]
        public void Compare_InvalidVersionCompatibility_Fails()
        {
            var dummyFileSystem = new FakeFileSystem();
            var mockLoader = new Mock<IReportLoader>();
            var mockComparer = new Mock<IResultComparer>();

            dummyFileSystem.AddFile("file1", "");
            dummyFileSystem.AddFile("file2", "");

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem, mockLoader.Object, mockComparer.Object)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionCompatibility = "invalid compat level",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeFalse();
            buildEngine.ErrorEvents.Count.Should().Be(1);
            buildEngine.ErrorEvents[0].Message.Contains("invalid compat level").Should().BeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void Compare_ComparerReturnsTrue_TaskSucceeds(bool comparerReturnValue, bool expectedTaskResult)
        {
            var dummyFileSystem = new FakeFileSystem();
            var mockLoader = new Mock<IReportLoader>(MockBehavior.Strict);
            var mockComparer = new Mock<IResultComparer>(MockBehavior.Strict);

            dummyFileSystem.AddFile("c:\\file1.txt", "aaa");
            dummyFileSystem.AddFile("c:\\file2.txt", "bbb");

            var result1 = new AsmRefResult(new InputCriteria(), Array.Empty<SourceAssemblyInfo>());
            var result2 = new AsmRefResult(new InputCriteria(), Array.Empty<SourceAssemblyInfo>());

            mockLoader.Setup(l => l.Load("aaa")).Returns(result1);
            mockLoader.Setup(l => l.Load("bbb")).Returns(result2);

            ComparisonOptions actualOptions = null;
            mockComparer.Setup(c => c.AreSame(result1, result2, It.IsAny<ComparisonOptions>()))
                .Callback((AsmRefResult r1, AsmRefResult r2, ComparisonOptions options) => actualOptions = options)
                .Returns(comparerReturnValue);

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem,
                mockLoader.Object, mockComparer.Object)
            {
                BaseLineReportFilePath = "c:\\file1.txt",
                CurrentReportFilePath = "c:\\file2.txt",
                VersionCompatibility = "MajorMinorBuild",
                IgnoreSourcePublicKeyToken = true,
                BuildEngine = buildEngine
            };

            testSubject.Execute().Should().Be(expectedTaskResult);
            mockLoader.VerifyAll();
            mockComparer.VerifyAll();
            actualOptions.VersionCompatibility.Should().Be(VersionCompatibility.MajorMinorBuild);
            actualOptions.IgnoreSourcePublicKeyToken.Should().BeTrue();

            if (expectedTaskResult)
            {
                buildEngine.ErrorEvents.Should().BeEmpty();
            }
            else
            {
                buildEngine.ErrorEvents.Count.Should().Be(1);
            }
        }

        [Fact]
        public void Compare_EndToEnd_Same_NoError()
        {
            var dummyFileSystem = new FakeFileSystem();
            const string reportContent = @"---

# Base directory: d:\repos\devtility\asmref-baseliner
Include patterns:
- src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe
Exclude patterns:
- src\**.dll
- xxx\yyy.dll
# Number of matches: 2

---

Assembly: DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null
Relative path: src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe

Referenced assemblies:   # count = 2
- Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
- System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089

---

Assembly: Assembly2, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
Relative path: asm2/bin/Debug/net461/assembly2.dll

Referenced assemblies:   # count = 1
- mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089

...";

            dummyFileSystem.AddFile("file1", reportContent);
            dummyFileSystem.AddFile("file2", reportContent);

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem, 
                new YamlReportLoader(), new AsmRefResultComparer())
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionCompatibility = "sTRICt",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeTrue();
            buildEngine.ErrorEvents.Count.Should().Be(0);
            buildEngine.MessageEvents.Count.Should().Be(4);
            buildEngine.MessageEvents[1].Message.Contains(": Strict").Should().BeTrue();
            buildEngine.MessageEvents[2].Message.Contains(": False").Should().BeTrue();
            buildEngine.MessageEvents[3].Message.Contains(": file1").Should().BeTrue();
            buildEngine.MessageEvents[3].Message.Contains(": file2").Should().BeTrue();
        }

        [Fact]
        public void Compare_EndToEnd_Different_Error()
        {
            var reportContent1 = @"---
Include patterns:
- src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe
Exclude patterns:
- src\**.dll

---

Assembly: DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null
Relative path: src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe

Referenced assemblies:   # count = 2
- Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
- System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089

...";

            var reportContent2 = @"---

Include patterns:
- src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe
Exclude patterns:
- src\**.dll

---

Assembly: DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null
Relative path: src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe

Referenced assemblies:   # count = 1
- Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a

...";

            var dummyFileSystem = new FakeFileSystem();
            dummyFileSystem.AddFile("file1", reportContent1);
            dummyFileSystem.AddFile("file2", reportContent2);

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem,
                new YamlReportLoader(), new AsmRefResultComparer())
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionCompatibility = "any",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeFalse();
            buildEngine.ErrorEvents.Count.Should().Be(1);
            buildEngine.ErrorEvents[0].Message.Contains("file1").Should().BeTrue();
            buildEngine.ErrorEvents[0].Message.Contains("file2").Should().BeTrue();
        }
    }
}
