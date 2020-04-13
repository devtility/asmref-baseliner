// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.MSBuild.Tests
{
    internal static class MSBuildLocatorInitializer
    {
        static MSBuildLocatorInitializer()
        {
            // Must be done in a separate method, before any code that uses the
            // Microsoft.Build namespace.
            // See https://github.com/microsoft/MSBuildLocator/commit/f3d5b0814bc7c5734d03a617c17c6998dd2f0e99
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
        }

        public static void EnsureMSBuildInitialized()
        {
            // no-op - the work is done in the static constructor
        }
    }
}
