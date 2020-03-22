// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class YamlReportLoaderTests
    {
        [Fact]
        public void Deserialize_InputCriteria()
        {
            var data = @"
# Base directory: d:\repos\devtility\asmref-baseliner
Include patterns:
- src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe
Exclude patterns:
# Number of matches: 2

";
            var actual = YamlReportLoader.Deserialize<InputCriteria>(data);

            actual.Should().NotBeNull();

            actual.BaseDirectory.Should().BeNull();
            actual.IncludePatterns.Should().BeEquivalentTo(@"src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe");
            actual.ExcludePatterns.Should().BeNull();
        }

        [Fact]
        public void Deserialize_InputCriteria_NoData_NullReturned()
        {
            var data = @"
# Base directory: d:\repos\devtility\asmref-baseliner
# Include patterns: src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe
# Exclude patterns:
# Number of matches: 2

";
            var actual = YamlReportLoader.Deserialize<InputCriteria>(data);

            actual.Should().BeNull();
        }

        [Fact]
        public void Deserialize_AssemblyReferenceInfo()
        {
            var data = @"
Assembly: DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null
Relative path: src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe
Assembly load exception: a load exception. xxx

Referenced assemblies:   # count = 2
- Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
- System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
";
            var actual = YamlReportLoader.Deserialize<SourceAssemblyInfo>(data);

            actual.Should().NotBeNull();

            actual.AssemblyName.Should().Be("DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null");
            actual.RelativePath.Should().Be("src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe");
            actual.LoadException.Should().Be("a load exception. xxx");

            actual.ReferencedAssemblies.Should().BeEquivalentTo(
                "Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        [Fact]  
        public void Deserialize()
        {
            var data = @"---

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

...
";
            var testSubject = new YamlReportLoader();
            
            var result = testSubject.Load(data);

            result.Should().NotBeNull();
            result.InputCriteria.BaseDirectory.Should().BeNull();
            result.InputCriteria.IncludePatterns.Should().BeEquivalentTo(@"src\DumpAsmRefs\bin\Debug\net461\DumpAsmRefs.exe");
            result.InputCriteria.ExcludePatterns.Should().BeEquivalentTo(@"src\**.dll", @"xxx\yyy.dll");

            result.SourceAssemblyInfos.Should().NotBeNull();

            var refs = result.SourceAssemblyInfos.ToArray();

            refs[0].LoadException.Should().BeNull();
            refs[0].FullPath.Should().BeNull();
            refs[0].RelativePath.Should().Be("src/DumpAsmRefs/bin/Debug/net461/DumpAsmRefs.exe");
            refs[0].AssemblyName.Should().Be("DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null");
            refs[0].ReferencedAssemblies.Should().NotBeNull();
            refs[0].ReferencedAssemblies.ToArray()[0].Should().Be("Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            refs[0].ReferencedAssemblies.ToArray()[1].Should().Be("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            refs[0].ReferencedAssemblies.Count().Should().Be(2);

            refs[1].LoadException.Should().BeNull();
            refs[1].FullPath.Should().BeNull();
            refs[1].RelativePath.Should().Be("asm2/bin/Debug/net461/assembly2.dll");
            refs[1].AssemblyName.Should().Be("Assembly2, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null");
            refs[1].ReferencedAssemblies.Should().NotBeNull();
            refs[1].ReferencedAssemblies.ToArray()[0].Should().Be("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            refs[1].ReferencedAssemblies.Count().Should().Be(1);
        }

        [Fact]
        public void Roundtrip_BuildReportThenReload()
        {
            var inputs = new InputCriteria("BASE DIR", new string[] { "**\\*Console*" },
                new string[] { "**\\exclude1*" });

            var asmRefInfos = new SourceAssemblyInfo[]
            {
                new SourceAssemblyInfo()
                {
                    LoadException = null,
                    AssemblyName = "asmName1",
                    RelativePath = "relative path1",
                    ReferencedAssemblies = new string[]{ "asm 1_1", "asm 1_2" }
                },
                new SourceAssemblyInfo()
                {
                    LoadException = "exception message",
                    AssemblyName = "asmName2",
                    RelativePath = "relative path2",
                    ReferencedAssemblies = new string[] { "asm 2_1", "asm 2_2" }
                }
            };

            var asmResult = new AsmRefResult(inputs, asmRefInfos);

            // Build report
            var reportBuilder = new YamlReportBuilder();
            string data = reportBuilder.Generate(asmResult);

            // Reload report
            var loader = new YamlReportLoader();
            var actual = loader.Load(data);

            // Assert
            actual.SourceAssemblyInfos.Should().BeEquivalentTo(asmResult.SourceAssemblyInfos);
        }
    }
}
