// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DumpAsmRefs
{
    public class AssemblyInfoGenerator
    {
        public IList<AssemblyReferenceInfo> Fetch(string baseDirectory, IEnumerable<string> relativeFilePaths)
        {
            var results = new List<AssemblyReferenceInfo>();

            foreach(var relativePath in relativeFilePaths)
            {
                var fullPath = Path.Combine(baseDirectory, relativePath);
                var assembly = LoadAssembly(fullPath);

                var newResult = new AssemblyReferenceInfo
                {
                    SourceAssemblyFullPath = fullPath,
                    SourceAssemblyRelativePath = relativePath,
                    SourceAssemblyName = assembly.GetName(),
                    ReferencedAssemblies = assembly.GetReferencedAssemblies()
                };

                results.Add(newResult);
            }
            return results;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used",
            Justification = "The user has given us the file path to specify the assembly they want us to load")]
        protected virtual Assembly LoadAssembly(string fullFilePath)
        {
            return Assembly.LoadFrom(fullFilePath);
        }
    }

    public class AssemblyReferenceInfo
    {
        public string SourceAssemblyFullPath { get; set; }
        public string SourceAssemblyRelativePath { get; set; }

        public AssemblyName SourceAssemblyName { get; set; }
        public IReadOnlyList<AssemblyName> ReferencedAssemblies { get; set; }
    }
}
