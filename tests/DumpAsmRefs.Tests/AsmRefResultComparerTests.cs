﻿// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
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
            AsmRefResultComparer.AreListsSame(list1, list2).Should().Be(expected);
            AsmRefResultComparer.AreListsSame(list2, list1).Should().Be(expected);
        }

        [Fact]
        public void AreSame_Inputs_IncludeAndExcludeAreSame()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "123" },
                ExcludePatterns = new string[] { "123" },
                RelativeFilePaths = new string[] { "any - should be ignored" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "123" },
                ExcludePatterns = new string[] { "123" },
                RelativeFilePaths = null
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_Inputs_IncludesDiffer()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1" },
                RelativeFilePaths = new string[] { "any - should be ignored" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "incl1", "incl2", "incl3" },
                ExcludePatterns = new string[] { "excl1" },
                RelativeFilePaths = null
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeFalse();
        }

        [Fact]
        public void AreSame_Inputs_ExcludesDiffer()
        {
            var input1 = new InputCriteria
            {
                BaseDirectory = "base - should be ignored",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1" },
                RelativeFilePaths = new string[] { "any - should be ignored" }
            };

            var input2 = new InputCriteria
            {
                BaseDirectory = "XXX XXX",
                IncludePatterns = new string[] { "incl1", "incl2" },
                ExcludePatterns = new string[] { "excl1", "EXCL2" },
                RelativeFilePaths = null
            };

            AsmRefResultComparer.AreSame(input1, input2).Should().BeFalse();
        }

        [Fact]
        public void AreSame_AsmRefInfo_Same()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();

            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_SourceFullPath_IsIgnored()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyFullPath = "path 1";
            ref2.SourceAssemblyFullPath = "path 2";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_SourceAssemblyName()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyName = "modified name";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeFalse();

            // 2. Same
            ref2.SourceAssemblyName = "modified name";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_RelativePath()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();

            // 1. Different
            ref1.SourceAssemblyRelativePath = "modified path";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeFalse();

            // 2. Same
            ref2.SourceAssemblyRelativePath = "modified path";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_ReferencedAssemblies()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();

            // 1. Different
            ref1.ReferencedAssemblies = new string[] { "mod1", "mod2" };
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeFalse();

            // 2. Same
            ref2.ReferencedAssemblies = new string[] { "mod1", "mod2" };
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
        }

        [Fact]
        public void AreSame_AsmRefInfo_WithExceptions()
        {
            var ref1 = CreateWellKnownAsmRefInfo();
            var ref2 = CreateWellKnownAsmRefInfo();
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();

            ref1.ReferencedAssemblies = null;
            ref2.ReferencedAssemblies = null;

            // 1. Differ by exception
            ref1.LoadException = "exc1";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeFalse();

            // 2. Same exception info
            ref2.LoadException = "exc1";
            AsmRefResultComparer.AreSame(ref1, ref2).Should().BeTrue();
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
    }
}