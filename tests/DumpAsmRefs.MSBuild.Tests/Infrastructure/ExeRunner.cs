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

            Console.WriteLine($"ExeRunner: FileName: {exePath}");
            Console.WriteLine($"ExeRunner: Arguments: {arguments}");

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

            EnsureProcessHasExited(process, timeoutInMs);

            var result = new ExecutionResult(process.ExitCode, executionStatus,
                process.StandardOutput.ReadToEnd(),
                process.StandardError.ReadToEnd());

            EnsureProcessEnded(process);
            return result;
        }

        private static void EnsureProcessHasExited(Process process, int timeoutInMs)
        {
            var exited = process.WaitForExit(timeoutInMs);
            Console.WriteLine($"{nameof(EnsureProcessHasExited)}: WaitForExit result = {exited}");
            if (!exited)
            {
                int retryCount = 0;
                while(retryCount < 5 && !process.HasExited)
                {
                    Console.WriteLine($"Waiting for process to exit: {retryCount}");
                    System.Threading.Thread.Sleep(10000);
                    retryCount++;
                }
                Console.WriteLine($"Finished waiting for process to exit. HasExited = {process.HasExited}");
            }
        }

        private static void EnsureProcessEnded(Process process)
        {
            if (process.HasExited)
            {
                Console.WriteLine($"{nameof(EnsureProcessEnded)}: process has already ended");
                return;
            }

            try
            {
                Console.WriteLine($"{nameof(EnsureProcessEnded)}: killing the process...");
                process.Kill(); // asynchronous
                process.WaitForExit();
                Console.WriteLine($"{nameof(EnsureProcessEnded)}: Process killed. HasExited = {process.HasExited}");
            }
            catch (Exception)
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
