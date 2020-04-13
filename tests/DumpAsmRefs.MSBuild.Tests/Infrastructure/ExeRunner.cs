// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Diagnostics;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal static class ExeRunner
    {
        private const int DefaultTimeoutInMs = 1000 * 60 * 2;

        public static ExecutionResult Run(string exePath, string arguments = null, int timeoutInMs = DefaultTimeoutInMs)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    CreateNoWindow = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    ErrorDialog = false
                }
            };

            ExecutionStatus executionStatus;
            if (!process.Start())
            {
                executionStatus = ExecutionStatus.FailedToStart;
            }
            else
            {
                executionStatus = process.WaitForExit(timeoutInMs)
                    ? ExecutionStatus.Completed
                    : ExecutionStatus.TimedOut;
            }

            var result = new ExecutionResult(process.ExitCode, executionStatus,
                process.StandardOutput.ReadToEnd(),
                process.StandardError.ReadToEnd());

            EnsureProcessEnded(process);
            return result;
        }

        private static void EnsureProcessEnded(Process process)
        {
            if (process.HasExited)
            {
                return;
            }

            try
            {
                process.Kill();
            }
            catch(Exception)
            {
                // squash
            }
        }

        public enum ExecutionStatus
        {
            FailedToStart,
            TimedOut,
            Completed
        }

        public class ExecutionResult
        {
            public ExecutionResult(int exitCode, ExecutionStatus executionStatus,
                string standardOutput, string standardError)
            {
                ExitCode = exitCode;
                Status = executionStatus;
                StandardOutput = standardOutput;
                StandardError = standardError;
            }

            public int ExitCode { get; }
            public ExecutionStatus Status { get; }
            public string StandardOutput { get; }
            public string StandardError { get; }
        }
    }
}
