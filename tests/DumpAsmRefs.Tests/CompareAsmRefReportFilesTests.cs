// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class CompareAsmRefReportFilesTests
    {
        [Fact]
        public void Compare_InvalidStrictness_Fails()
        {
            var dummyFileSystem = new FakeFileSystem();

            dummyFileSystem.AddFile("file1", "");
            dummyFileSystem.AddFile("file2", "");

            var buildEngine = new FakeBuildEngine();

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionStrictness = "invalid strictness",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeFalse();
            buildEngine.ErrorEvents.Count.Should().Be(1);
            buildEngine.ErrorEvents[0].Message.Contains("invalid strictness").Should().BeTrue();
        }

        [Fact]
        public void Compare_Same_NoError()
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

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionStrictness = "sTRICt",
                BuildEngine = buildEngine
            };

            // Test
            bool result = testSubject.Execute();

            // Check
            result.Should().BeTrue();
            buildEngine.ErrorEvents.Count.Should().Be(0);
            buildEngine.MessageEvents.Count.Should().Be(2);
            buildEngine.MessageEvents[0].Message.Contains("Strict").Should().BeTrue();
            buildEngine.MessageEvents[1].Message.Contains("file1").Should().BeTrue();
            buildEngine.MessageEvents[1].Message.Contains("file2").Should().BeTrue();
        }

        [Fact]
        public void Compare_Different_Error()
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

            var testSubject = new CompareAsmRefReportFiles(dummyFileSystem)
            {
                BaseLineReportFilePath = "file1",
                CurrentReportFilePath = "file2",
                VersionStrictness = "any",
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
