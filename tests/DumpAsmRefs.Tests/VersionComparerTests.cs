// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class VersionComparerTests
    {
        [Theory]
        [InlineData(VersionCompatibility.Any)]
        [InlineData(VersionCompatibility.Major)]
        [InlineData(VersionCompatibility.MajorMinor)]
        [InlineData(VersionCompatibility.MajorMinorBuild)]
        [InlineData(VersionCompatibility.Strict)]
        public void CompareVersions_NullNullAreEqual(VersionCompatibility versionCompatibility)
        {
            VersionComparer.AreVersionsEqual(null, null, versionCompatibility).Should().Be(true);
        }

        [Theory]
        [InlineData(VersionCompatibility.Major)]
        [InlineData(VersionCompatibility.MajorMinor)]
        [InlineData(VersionCompatibility.MajorMinorBuild)]
        [InlineData(VersionCompatibility.Strict)]
        public void CompareVersions_NullNonNullAreDifferent(VersionCompatibility versionCompatibility)
        {
            var version = new Version("1.0");
            VersionComparer.AreVersionsEqual(null, version, versionCompatibility).Should().Be(false);
            VersionComparer.AreVersionsEqual(version, null, versionCompatibility).Should().Be(false);
        }

        [Fact]
        public void CompareVersions_NullNonNullAreSame_ForCompatibilityAny()
        {
            var version = new Version("1.0");
            VersionComparer.AreVersionsEqual(null, version, VersionCompatibility.Any).Should().Be(true);
            VersionComparer.AreVersionsEqual(version, null, VersionCompatibility.Any).Should().Be(true);
        }

        [Theory]
        [InlineData("1.2", VersionCompatibility.Any)]
        [InlineData("1.2", VersionCompatibility.Major)]
        [InlineData("1.2", VersionCompatibility.MajorMinor)]
        [InlineData("1.2", VersionCompatibility.MajorMinorBuild)]
        [InlineData("1.2", VersionCompatibility.Strict)]

        [InlineData("1.2.3", VersionCompatibility.Any)]
        [InlineData("1.2.3", VersionCompatibility.Major)]
        [InlineData("1.2.3", VersionCompatibility.MajorMinor)]
        [InlineData("1.2.3", VersionCompatibility.MajorMinorBuild)]
        [InlineData("1.2.3", VersionCompatibility.Strict)]

        [InlineData("1.2.3.4", VersionCompatibility.Any)]
        [InlineData("1.2.3.4", VersionCompatibility.Major)]
        [InlineData("1.2.3.4", VersionCompatibility.MajorMinor)]
        [InlineData("1.2.3.4", VersionCompatibility.MajorMinorBuild)]
        [InlineData("1.2.3.4", VersionCompatibility.Strict)]
        public void CompareVersions_SameVersionSucceeds(string version, VersionCompatibility versionCompatibility)
            => CompareAndCheck(version, version, versionCompatibility, true);

        [Theory]
        [InlineData("1.2", "1.3", VersionCompatibility.Any, true)]
        [InlineData("1.2", "1.3", VersionCompatibility.Major, true)]
        [InlineData("1.2", "1.3", VersionCompatibility.MajorMinor, false)]
        [InlineData("1.2", "1.3", VersionCompatibility.MajorMinorBuild, false)]
        [InlineData("1.2", "1.3", VersionCompatibility.Strict, false)]

        [InlineData("1.2", "1.2.3", VersionCompatibility.Any, true)]
        [InlineData("1.2", "1.2.3", VersionCompatibility.Major, true)]
        [InlineData("1.2", "1.2.3", VersionCompatibility.MajorMinor, true)]
        [InlineData("1.2", "1.2.3", VersionCompatibility.MajorMinorBuild, false)]
        [InlineData("1.2", "1.2.3", VersionCompatibility.Strict, false)]

        [InlineData("1.2", "1.2.3.4", VersionCompatibility.Any, true)]
        [InlineData("1.2", "1.2.3.4", VersionCompatibility.Major, true)]
        [InlineData("1.2", "1.2.3.4", VersionCompatibility.MajorMinor, true)]
        [InlineData("1.2", "1.2.3.4", VersionCompatibility.MajorMinorBuild, false)]
        [InlineData("1.2", "1.2.3.4", VersionCompatibility.Strict, false)]

        [InlineData("9.1", "10.1", VersionCompatibility.Any, true)]
        [InlineData("9.1", "10.1", VersionCompatibility.Major, false)]
        [InlineData("9.1", "10.1", VersionCompatibility.MajorMinor, false)]
        [InlineData("9.1", "10.1", VersionCompatibility.MajorMinorBuild, false)]
        [InlineData("9.1", "10.1", VersionCompatibility.Strict, false)]
        public void CompareAndCheck_Symmetrical(string version1, string version2, VersionCompatibility versionCompatibility, bool expected)
        {
#pragma warning disable S2234 // Parameters should be passed in the correct order
            CompareAndCheck(version1, version2, versionCompatibility, expected);
            CompareAndCheck(version2, version1, versionCompatibility, expected);
#pragma warning restore S2234 // Parameters should be passed in the correct order
        }

        private static void CompareAndCheck(string version1, string version2, VersionCompatibility versionCompatibility, bool expected)
        {
            var first = new Version(version1);
            var second = new Version(version2);
            VersionComparer.AreVersionsEqual(first, second, versionCompatibility).Should().Be(expected);
        }

    }
}
