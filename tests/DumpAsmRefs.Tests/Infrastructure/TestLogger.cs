// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.Collections.Generic;

namespace DumpAsmRefs.Tests.Infrastructure
{
    internal class TestLogger : ILogger
    {
        public Verbosity Verbosity { get; set; }
        public IList<string> DebugMessages { get; } = new List<string>();
        public IList<string> InfoMessages { get; } = new List<string>();
        public IList<string> NormalMessages { get; } = new List<string>();
        public IList<string> Warnings { get; } = new List<string>();
        public IList<string> Errors { get; } = new List<string>();

        #region ILogger implementation

        void ILogger.LogDebug(string message, params object[] arguments)
        {
            DebugMessages.Add(string.Format(message, arguments));
        }

        void ILogger.LogError(string message, params object[] arguments)
        {
            Errors.Add(string.Format(message, arguments));
        }

        void ILogger.LogInfo(string message, params object[] arguments)
        {
            InfoMessages.Add(string.Format(message, arguments));
        }

        void ILogger.LogMessage(string message, params object[] arguments)
        {
            NormalMessages.Add(string.Format(message, arguments));
        }

        void ILogger.LogWarning(string message, params object[] arguments)
        {
            Warnings.Add(string.Format(message, arguments));
        }

        #endregion ILogger implementation
    }
}
