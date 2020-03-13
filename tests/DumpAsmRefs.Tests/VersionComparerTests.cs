// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class VersionComparerTests
    {
        [Theory]
        [InlineData(VersionComparisonStrictness.Any)]
        [InlineData(VersionComparisonStrictness.Major)]
        [InlineData(VersionComparisonStrictness.MajorMinor)]
        [InlineData(VersionComparisonStrictness.MajorMinorBuild)]
        [InlineData(VersionComparisonStrictness.Strict)]
        public void CompareVersions_NullNullAreEqual(VersionComparisonStrictness strictness)
        {
            VersionComparer.AreVersionsEqual(null, null, strictness).Should().Be(true);
        }

        [Theory]
        [InlineData(VersionComparisonStrictness.Major)]
        [InlineData(VersionComparisonStrictness.MajorMinor)]
        [InlineData(VersionComparisonStrictness.MajorMinorBuild)]
        [InlineData(VersionComparisonStrictness.Strict)]
        public void CompareVersions_NullNonNullAreDifferent(VersionComparisonStrictness strictness)
        {
            var version = new Version("1.0");
            VersionComparer.AreVersionsEqual(null, version, strictness).Should().Be(false);
            VersionComparer.AreVersionsEqual(version, null, strictness).Should().Be(false);
        }

        [Fact]
        public void CompareVersions_NullNonNullAreSame_ForStrictnessAny()
        {
            var version = new Version("1.0");
            VersionComparer.AreVersionsEqual(null, version, VersionComparisonStrictness.Any).Should().Be(true);
            VersionComparer.AreVersionsEqual(version, null, VersionComparisonStrictness.Any).Should().Be(true);
        }

        [Theory]
        [InlineData("1.2", VersionComparisonStrictness.Any)]
        [InlineData("1.2", VersionComparisonStrictness.Major)]
        [InlineData("1.2", VersionComparisonStrictness.MajorMinor)]
        [InlineData("1.2", VersionComparisonStrictness.MajorMinorBuild)]
        [InlineData("1.2", VersionComparisonStrictness.Strict)]

        [InlineData("1.2.3", VersionComparisonStrictness.Any)]
        [InlineData("1.2.3", VersionComparisonStrictness.Major)]
        [InlineData("1.2.3", VersionComparisonStrictness.MajorMinor)]
        [InlineData("1.2.3", VersionComparisonStrictness.MajorMinorBuild)]
        [InlineData("1.2.3", VersionComparisonStrictness.Strict)]

        [InlineData("1.2.3.4", VersionComparisonStrictness.Any)]
        [InlineData("1.2.3.4", VersionComparisonStrictness.Major)]
        [InlineData("1.2.3.4", VersionComparisonStrictness.MajorMinor)]
        [InlineData("1.2.3.4", VersionComparisonStrictness.MajorMinorBuild)]
        [InlineData("1.2.3.4", VersionComparisonStrictness.Strict)]
        public void CompareVersions_SameVersionSucceeds(string version, VersionComparisonStrictness strictness)
            => CompareAndCheck(version, version, strictness, true);

        [Theory]
        [InlineData("1.2", "1.3", VersionComparisonStrictness.Any, true)]
        [InlineData("1.2", "1.3", VersionComparisonStrictness.Major, true)]
        [InlineData("1.2", "1.3", VersionComparisonStrictness.MajorMinor, false)]
        [InlineData("1.2", "1.3", VersionComparisonStrictness.MajorMinorBuild, false)]
        [InlineData("1.2", "1.3", VersionComparisonStrictness.Strict, false)]

        [InlineData("1.2", "1.2.3", VersionComparisonStrictness.Any, true)]
        [InlineData("1.2", "1.2.3", VersionComparisonStrictness.Major, true)]
        [InlineData("1.2", "1.2.3", VersionComparisonStrictness.MajorMinor, true)]
        [InlineData("1.2", "1.2.3", VersionComparisonStrictness.MajorMinorBuild, false)]
        [InlineData("1.2", "1.2.3", VersionComparisonStrictness.Strict, false)]

        [InlineData("1.2", "1.2.3.4", VersionComparisonStrictness.Any, true)]
        [InlineData("1.2", "1.2.3.4", VersionComparisonStrictness.Major, true)]
        [InlineData("1.2", "1.2.3.4", VersionComparisonStrictness.MajorMinor, true)]
        [InlineData("1.2", "1.2.3.4", VersionComparisonStrictness.MajorMinorBuild, false)]
        [InlineData("1.2", "1.2.3.4", VersionComparisonStrictness.Strict, false)]

        [InlineData("9.1", "10.1", VersionComparisonStrictness.Any, true)]
        [InlineData("9.1", "10.1", VersionComparisonStrictness.Major, false)]
        [InlineData("9.1", "10.1", VersionComparisonStrictness.MajorMinor, false)]
        [InlineData("9.1", "10.1", VersionComparisonStrictness.MajorMinorBuild, false)]
        [InlineData("9.1", "10.1", VersionComparisonStrictness.Strict, false)]
        public void CompareAndCheck_Symmetrical(string version1, string version2, VersionComparisonStrictness strictness, bool expected)
        {
#pragma warning disable S2234 // Parameters should be passed in the correct order
            CompareAndCheck(version1, version2, strictness, expected);
            CompareAndCheck(version2, version1, strictness, expected);
#pragma warning restore S2234 // Parameters should be passed in the correct order
        }

        private static void CompareAndCheck(string version1, string version2, VersionComparisonStrictness strictness, bool expected)
        {
            var first = new Version(version1);
            var second = new Version(version2);
            VersionComparer.AreVersionsEqual(first, second, strictness).Should().Be(expected);
        }

    }
}
