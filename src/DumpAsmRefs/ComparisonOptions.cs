// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs
{
    public class ComparisonOptions
    {
        public ComparisonOptions(VersionCompatibility versionCompatibility)
            :this(versionCompatibility, ignoreSourcePublicKeyToken: false, versionCompatibility ) {}

        public ComparisonOptions(VersionCompatibility sourceVersionCompatibility, bool ignoreSourcePublicKeyToken,
            VersionCompatibility targetVersionCompatibility)
        {
            SourceVersionCompatibility = sourceVersionCompatibility;
            IgnoreSourcePublicKeyToken = ignoreSourcePublicKeyToken;

            TargetVersionCompatibility = targetVersionCompatibility;
        }

        public VersionCompatibility SourceVersionCompatibility { get; }
        public bool IgnoreSourcePublicKeyToken { get; }

        public VersionCompatibility TargetVersionCompatibility { get; }
    }
}
