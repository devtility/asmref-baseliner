// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class DotNetSdk
    {
        public DotNetSdk(string versionText, string path)
        {
            VersionText = versionText;
            Path = path;

            if (Version.TryParse(versionText, out var parsedVersion))
            {
                Version = parsedVersion;
            }
        }

        public string VersionText { get; }
        public string Path { get; }
        public Version Version { get; }
    }
}
