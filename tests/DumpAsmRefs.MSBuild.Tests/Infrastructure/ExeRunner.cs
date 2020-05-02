// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace DumpAsmRefs.MSBuild.Tests
{
    internal class ExeRunner
    {
        private const int DefaultTimeoutInMs = 1000 * 60 * 1;
        private const int ShutdownTimeoutInMs = 1000 * 20;

        private readonly ITestOutputHelper logger;
        public ExeRunner(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        public ExecutionResult Run(string exePath, string arguments = null, int timeoutInMs = DefaultTimeoutInMs)
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

            logger.WriteLine($"ExeRunner: FileName: {exePath}");
            logger.WriteLine($"ExeRunner: Arguments: {arguments}");
            logger.WriteLine($"ExeRunner: Timeout: {timeoutInMs}");

            // Note: we're redirecting the IO streams, so we need to process the
            // data otherwise the process will hang if the output buffers are full
            var outputSb = new StringBuilder();
            var errorSb = new StringBuilder();

            process.OutputDataReceived += (s, args) => outputSb.AppendLine(args.Data);
            process.ErrorDataReceived += (s, args) => errorSb.AppendLine(args.Data);

            if (!process.Start())
            {
                executionStatus = ExecutionStatus.FailedToStart;
            }
            else
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                executionStatus = process.WaitForExit(timeoutInMs)
                    ? ExecutionStatus.Completed
                    : ExecutionStatus.TimedOut;
            }

            EnsureProcessHasExited(process, ShutdownTimeoutInMs);
            EnsureProcessEnded(process);

            var result = new ExecutionResult(process.ExitCode, executionStatus,
                outputSb.ToString(),
                errorSb.ToString());

            return result;
        }

        private void EnsureProcessHasExited(Process process, int timeoutInMs)
        {
            var exited = process.WaitForExit(timeoutInMs);
            logger.WriteLine($"{nameof(EnsureProcessHasExited)}: WaitForExit result = {exited}");
            if (!exited)
            {
                int retryCount = 0;
                while(retryCount < 5 && !process.HasExited)
                {
                    logger.WriteLine($"Waiting for process to exit: {retryCount}");
                    System.Threading.Thread.Sleep(10000);
                    retryCount++;
                }
                logger.WriteLine($"Finished waiting for process to exit. HasExited = {process.HasExited}");
            }
        }

        private void EnsureProcessEnded(Process process)
        {
            if (process.HasExited)
            {
                logger.WriteLine($"{nameof(EnsureProcessEnded)}: process has already ended");
                return;
            }

            try
            {
                logger.WriteLine($"{nameof(EnsureProcessEnded)}: killing the process...");
                process.Kill(); // asynchronous
                process.WaitForExit();
                logger.WriteLine($"{nameof(EnsureProcessEnded)}: Process killed. HasExited = {process.HasExited}");
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
