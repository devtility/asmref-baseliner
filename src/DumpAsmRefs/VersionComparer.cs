// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;

namespace DumpAsmRefs
{
    /// <summary>
    /// The level of strictness of to use when deciding if two versions should
    /// vbe considered equal
    /// </summary>
    public enum VersionCompatibility
    {
        Strict,
        Major,
        MajorMinor,
        MajorMinorBuild,

        /// <summary>
        /// Versions will always be treated as equal, even if one is null
        /// </summary>
        Any
    }

    public static class VersionComparer
    {
        public static bool AreVersionsEqual(Version first, Version second, VersionCompatibility versionCompatibility)
        {
            if (versionCompatibility == VersionCompatibility.Any)
            {
                return true;
            }

            // If either version is null then they will only be equal if both are null
            if (first == null || second == null)
            {
                return first == second;
            }

            switch (versionCompatibility)
            {
                case VersionCompatibility.Major:
                    return first.Major == second.Major;

                case VersionCompatibility.MajorMinor:
                    return first.Major == second.Major &&
                        first.Minor == second.Minor;

                case VersionCompatibility.MajorMinorBuild:
                    return first.Major == second.Major &&
                        first.Minor == second.Minor &&
                        first.Build == second.Build;

                default:
                    return first == second;
            }
        }
    }
}
