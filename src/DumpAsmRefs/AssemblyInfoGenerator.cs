﻿// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DumpAsmRefs
{
    public class AssemblyInfoGenerator : IAssemblyInfoGenerator
    {
        public IList<SourceAssemblyInfo> Fetch(string baseDirectory, IEnumerable<string> relativeFilePaths)
        {
            var results = new List<SourceAssemblyInfo>();

            foreach(var relativePath in relativeFilePaths)
            {
                var fullPath = System.IO.Path.Combine(baseDirectory, relativePath);

                Assembly assembly = null;
                Exception asmLoadException = null;
                try
                {
                    assembly = LoadAssembly(fullPath);
                }
                catch(Exception ex)
                {
                    asmLoadException = ex;
                }

                var newResult = new SourceAssemblyInfo
                {
                    LoadException = GetLoadExceptionText(asmLoadException),
                    FullPath = fullPath,
                    RelativePath = relativePath,
                    AssemblyName = assembly?.GetName().FullName,
                    ReferencedAssemblies = assembly?.GetReferencedAssemblies().Select(ra => ra.FullName).ToArray()
                };

                results.Add(newResult);
            }
            return results;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used",
            Justification = "The user has given us the file path to specify the assembly they want us to load")]
        protected virtual Assembly LoadAssembly(string fullFilePath)
        {
            // Note: the earliest version of .NET Core that supports Assembly.LoadFrom is v2.0.
            // See https://apisof.net/catalog/System.Reflection.Assembly.LoadFrom(String).
            return Assembly.LoadFrom(fullFilePath);
        }

        private static string GetLoadExceptionText(Exception ex)
        {
            var text = ex == null ? null : $"{ex.GetType().FullName}";
            if (ex is BadImageFormatException)
            {
                text += UIStrings.InfoGenerator_NotADotNetAssembly;
            }
            return text;
        }
    }
}
