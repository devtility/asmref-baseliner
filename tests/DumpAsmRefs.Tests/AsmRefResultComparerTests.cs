// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class AsmRefResultComparerTests
    {
        [Fact]
        public void AreListsSame_NullAndEmpty()
        {
            var emptyList = Array.Empty<string>();

            AsmRefResultComparer.AreListsSame(null, null).Should().BeTrue();
            AsmRefResultComparer.AreListsSame(null, emptyList).Should().BeTrue();
            AsmRefResultComparer.AreListsSame(emptyList, null).Should().BeTrue();
            AsmRefResultComparer.AreListsSame(emptyList, emptyList).Should().BeTrue();
        }

        [Theory]
        [InlineData(new string[] { "aaa" }, new string[] { "aaa" }, true)] // same, single element
        [InlineData(new string[] { "aaa" }, new string[] { "AAA" }, false)] // different case
        [InlineData(new string[] { "aaa" }, new string[] { "aaa", "bbb" }, false)] // different number of elements

        [InlineData(new string[] { "aaa", "bbb" }, new string[] { "aaa", "bbb" }, true)] // same, multiple elements
        [InlineData(new string[] { "aaa", "bbb" }, new string[] { "aaa", "BBB" }, false)] // different, case
        [InlineData(new string[] { "aaa", "c", "bbb" }, new string[] { "bbb", "aaa", "c" }, true)] // same, order should not matter
        public void AreListsSame_NonNull(string[] list1, string[] list2, bool expected)
        {
#pragma warning disable S2234 // Parameters should be passed in the correct order
            AsmRefResultComparer.AreListsSame(list1, list2).Should().Be(expected);
            AsmRefResultComparer.AreListsSame(list2, list1).Should().Be(expected);
#pragma warning restore S2234 // Parameters should be passed in the correct order
        }

        [Fact]
        public void AreSame_Inputs_IncludeAndExcludeAreSame()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "111" },
                ExcludePatterns = new string[] { "222" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "111" },
                ExcludePatterns = new string[] { "222" }
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeTrue();
            CompareReports(input1, input2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_Inputs_IncludesDiffer()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "incl1", "incl2", "incl3" },
                ExcludePatterns = new string[] { "excl1" }
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeFalse();
            CompareReports(input1, input2).Should().BeFalse();
        }

        [Fact]
        public void AreSame_Inputs_ExcludesDiffer()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1", "EXCL2" }
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeFalse();
            CompareReports(input1, input2).Should().BeFalse();
        }

        [Fact]
        public void AreSame_AsmRefInfo_Same()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();

            var options = new ComparisonOptions(VersionCompatibility.Strict);
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_SourceFullPath_IsIgnored()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyFullPath = "path 1";
            ref2.SourceAssemblyFullPath = "path 2";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_SourceAssemblyName_Strict()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyName = "modified name";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeFalse();

            // 2. Same
            ref2.SourceAssemblyName = "modified name";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_RelativePath()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyRelativePath = "modified path";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeFalse();

            // 2. Same
            ref2.SourceAssemblyRelativePath = "modified path";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_ReferencedAssemblies_Strict()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();

            // 1. Different
            ref1.ReferencedAssemblies = new string[] { "mod1", "mod2" };
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeFalse();

            // 2. Same
            ref2.ReferencedAssemblies = new string[] { "mod1", "mod2" };
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_WithExceptions()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();

            ref1.ReferencedAssemblies = null;
            ref2.ReferencedAssemblies = null;

            // 1. Differ by exception
            ref1.LoadException = "exc1";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeFalse();

            // 2. Same exception info
            ref2.LoadException = "exc1";
            AsmRefResultComparer.AreSame(ref1, ref2, options).Should().BeTrue();
        }

        [Fact]
        public void AreSame_Report_EmptyLists()
        {
            var input1 = CreateWellKnownInputCriteria();
            var input2 = CreateWellKnownInputCriteria();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            var report1 = new AsmRefResult(input1, Array.Empty<AssemblyReferenceInfo>());
            var report2 = new AsmRefResult(input2, Array.Empty<AssemblyReferenceInfo>());

            var testSubject = new AsmRefResultComparer();
            testSubject.AreSame(report1, report2, options)
                .Should().BeTrue();
        }

        [Fact]
        public void AreSame_Report_DifferentNumberOfSource()
        {
            var input1 = CreateWellKnownInputCriteria();
            var input2 = CreateWellKnownInputCriteria();
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            var report1 = new AsmRefResult(input1, new AssemblyReferenceInfo[]
                {
                    CreateAsmRefInfo("DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null")
                });

            var report2 = new AsmRefResult(input2, new AssemblyReferenceInfo[]
                {
                    CreateAsmRefInfo("DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null"),
                    CreateAsmRefInfo("Assembly2, Version=2.3.4.5, Culture=neutral, PublicKeyToken=null")
                });

            var testSubject = new AsmRefResultComparer();
            testSubject.AreSame(report1, report2, options)
                .Should().BeFalse();
        }

        [Theory]
        [InlineData(VersionCompatibility.Any, true)]
        [InlineData(VersionCompatibility.Major, true)]
        [InlineData(VersionCompatibility.MajorMinor, true)]
        [InlineData(VersionCompatibility.MajorMinorBuild, false)]
        [InlineData(VersionCompatibility.Strict, false)]
        public void AreSame_Report_SourcesDifferByVersion(VersionCompatibility versionCompatibility, bool expected)
        {
            var input1 = CreateWellKnownInputCriteria();
            var input2 = CreateWellKnownInputCriteria();
            var options = new ComparisonOptions(versionCompatibility);

            var report1 = new AsmRefResult(input1, new AssemblyReferenceInfo[]
                {
                    CreateAsmRefInfo("DumpAsmRefs, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null")
                });

            var report2 = new AsmRefResult(input2, new AssemblyReferenceInfo[]
                {
                    CreateAsmRefInfo("DumpAsmRefs, Version=1.2.8.9, Culture=neutral, PublicKeyToken=null"),
                });

            var testSubject = new AsmRefResultComparer();
            testSubject.AreSame(report1, report2, options)
                .Should().Be(expected);
        }

        [Theory]
        [InlineData(VersionCompatibility.Any, true)]
        [InlineData(VersionCompatibility.Major, true)]
        [InlineData(VersionCompatibility.MajorMinor, true)]
        [InlineData(VersionCompatibility.MajorMinorBuild, false)]
        [InlineData(VersionCompatibility.Strict, false)]
        public void AreSame_Report_SourcesDifferByRefdAsmVersion(VersionCompatibility versionCompatibility, bool expected)
        {
            var input1 = CreateWellKnownInputCriteria();
            var input2 = CreateWellKnownInputCriteria();
            var options = new ComparisonOptions(versionCompatibility);

            var source1 = new AssemblyReferenceInfo
            {
                SourceAssemblyName = "DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null",
                ReferencedAssemblies = new string[]
                {
                    "Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "System, Version=1.2.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                }
            };

            var source2 = new AssemblyReferenceInfo
            {
                SourceAssemblyName = "DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null",
                ReferencedAssemblies = new string[]
                {
                    "Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "System, Version=1.2.8.9, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                }
            };

            var report1 = new AsmRefResult(input1, new[] { source1 });

            var report2 = new AsmRefResult(input2, new[] { source2 });

            var testSubject = new AsmRefResultComparer();
            testSubject.AreSame(report1, report2, options)
                .Should().Be(expected);
        }

        private static bool CompareReports(InputCriteria first, InputCriteria second)
        {
            var report1 = new AsmRefResult(first, Enumerable.Empty<AssemblyReferenceInfo>());
            var report2 = new AsmRefResult(second, Enumerable.Empty<AssemblyReferenceInfo>());
            var options = new ComparisonOptions(VersionCompatibility.Strict);

            var testSubject = new AsmRefResultComparer();
            return testSubject.AreSame(report1, report2, options);
        }

        private static AssemblyReferenceInfo CreateWellKnownAsmRefInfo()
        {
            var asmRefInfo = new AssemblyReferenceInfo
            {
                SourceAssemblyFullPath = "should be ignored",
                SourceAssemblyName = "DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null",
                SourceAssemblyRelativePath = "..\\..\\sub1\\DumpAsmRefs.dll",
                LoadException = null,
                ReferencedAssemblies = new string[]
                {
                    "Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                }
            };

            return asmRefInfo;
        }

        private static AssemblyReferenceInfo CreateAsmRefInfo(string sourceAssemblyName)
        {
            var asmRefInfo = new AssemblyReferenceInfo
            {
                SourceAssemblyFullPath = "should be ignored",
                SourceAssemblyName = sourceAssemblyName,
                SourceAssemblyRelativePath = "..\\..\\sub1\\DumpAsmRefs.dll",
                LoadException = null,
                ReferencedAssemblies = new string[]
                {
                    "Microsoft.Build.Framework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                }
            };

            return asmRefInfo;
        }

        private static InputCriteria CreateWellKnownInputCriteria()
        {
            var input = new InputCriteria
            {
                BaseDirectory = "c:\\wellknown\\basedir\\",
                IncludePatterns = new string[] { "well known include1" },
                ExcludePatterns = new string[] { "well known exclude1" }
            };

            return input;
        }
    }
}
