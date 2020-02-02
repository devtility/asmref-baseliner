// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System;

namespace DumpAsmRefs
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger(Verbosity verbosity)
        {
            Verbosity = verbosity;
        }

        public Verbosity Verbosity { get; set; }

        #region ILogger implementation

        public void LogWarning(string message, params object[] arguments)
            => Log(Verbosity.Minimal, ConsoleColor.Yellow, message, arguments);

        public void LogError(string message, params object[] arguments)
            => Log(Verbosity.Minimal, ConsoleColor.Red, message, arguments);

        public void LogMessage(string message, params object[] arguments)
            => Log(Verbosity.Normal, null /* default */, message, arguments);

        public void LogInfo(string message, params object[] arguments)
            => Log(Verbosity.Detailed, ConsoleColor.Green, message, arguments);

        public void LogDebug(string message, params object[] arguments)
            => Log(Verbosity.Diagnostic, ConsoleColor.Cyan, message, arguments);

        #endregion ILogger implementation

        #region Private methods

        private void Log(Verbosity minimumVerbosity, ConsoleColor? color, string message, params object[] arguments)
        {
            if (Verbosity < minimumVerbosity)
            {
                return;
            }

            var currentConsoleColor = Console.ForegroundColor;
            try
            {
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }
                Console.WriteLine(message, arguments);
            }
            finally
            {
                Console.ForegroundColor = currentConsoleColor;
            }
        }

        #endregion
    }
}
