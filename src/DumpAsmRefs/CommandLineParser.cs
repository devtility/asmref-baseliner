// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;

namespace DumpAsmRefs
{
    internal class CommandLineParser
    {
        private const string DefaultOutputFileName = "ReferencedAssemblies.yml";

        public bool TryParse(ILogger logger, string[] args, out UserArguments userArguments)
        {
            userArguments = null;

            var app = new CommandLineApplication(false);

            var rootDirOption = app.Option("--root|-r", UIStrings.Parser_ArgDescription_Root, CommandOptionType.SingleValue);
            var fileOption = app.Option("--outfile|-o", UIStrings.Parser_ArgDescription_File, CommandOptionType.SingleValue);
            var verbosityOpt = app.Option("--verbosity|-v", UIStrings.Parser_ArgDescription_Verbosity, CommandOptionType.SingleValue);

            app.Execute(args);

            var fileName = fileOption.HasValue() ? fileOption.Value() : DefaultOutputFileName;
            var rootDir = rootDirOption.HasValue() ? rootDirOption.Value() : Directory.GetCurrentDirectory();
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(rootDir, fileName);
            }

            var verbosity = Verbosity.Normal;
            if (verbosityOpt.HasValue()
                && !Enum.TryParse<Verbosity>(verbosityOpt.Value(), true, out verbosity))
            {
                logger.LogError(UIStrings.Parser_Error_InvalidVerbosity, verbosityOpt.Value());
                return false;
            }

            var includeArgs = app.RemainingArguments.Where(x => !x.StartsWith("!")).ToArray();
            var excludeArgs = app.RemainingArguments.Where(x => x.StartsWith("!"))
                .Select(x => x.Substring(1))
                .ToArray();

            if (includeArgs.Length == 0)
            {
                logger.LogError(UIStrings.Parser_IncludePatternRequired);
                return false;
            }

            userArguments = new UserArguments(rootDir, includeArgs, excludeArgs, fileName, verbosity);
            return true;
        }
    }
}
