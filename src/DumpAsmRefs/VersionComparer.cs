// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;

namespace DumpAsmRefs
{
    /// <summary>
    /// The level of strictness of to use when deciding if two versions should
    /// vbe considered equal
    /// </summary>
    public enum VersionComparisonStrictness
    {
        Strict,
        Major,
        MajorMinor,
        MajorMinorBuild
    }

    public static class VersionComparer
    {
        public static bool AreVersionsEqual(Version first, Version second, VersionComparisonStrictness strictness)
        {
            // If either version is null then they will only be equal if both are null
            if (first == null || second == null)
            {
                return first == second;
            }

            switch (strictness)
            {
                case VersionComparisonStrictness.Major:
                    return first.Major == second.Major;

                case VersionComparisonStrictness.MajorMinor:
                    return first.Major == second.Major &&
                        first.Minor == second.Minor;

                case VersionComparisonStrictness.MajorMinorBuild:
                    return first.Major == second.Major &&
                        first.Minor == second.Minor &&
                        first.Build == second.Build;

                default:
                    return first == second;
            }
        }
    }
}
