// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.IO;
using System.Linq;

namespace DumpAsmRefs
{
    internal class CommandLineParser
    {
        private const string DefaultOutputFileName = "ReferencedAssemblies.yml";

        public bool TryParse(ILogger logger, string[] args, out UserArguments userArguments)
        {
            // TODO: parse arguments

            string outputFileName = null;
            string rootDirectory = Directory.GetCurrentDirectory();

            if (string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = DefaultOutputFileName;
            }
            if (!Path.IsPathRooted(outputFileName))
            {
                outputFileName = Path.Combine(rootDirectory, outputFileName);
            }

            userArguments = new UserArguments(rootDirectory, args, Enumerable.Empty<string>(), outputFileName, Verbosity.Diagnostic);

            return true;
        }
    }
}
