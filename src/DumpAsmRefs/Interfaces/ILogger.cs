// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.Interfaces
{
    internal interface ILogger
    {
        Verbosity Verbosity { get; set; }

        void LogError(string message, params object[] arguments);
        void LogWarning(string message, params object[] arguments);
        void LogInfo(string message, params object[] arguments);
        void LogMessage(string message, params object[] arguments);
        void LogDebug(string message, params object[] arguments);
    }

    public enum Verbosity
    {
        Minimal,
        Normal,
        Detailed,
        Diagnostic
    }
}
