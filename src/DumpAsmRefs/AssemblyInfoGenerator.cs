// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DumpAsmRefs
{
    public class AssemblyInfoGenerator
    {
        public IList<AssemblyReferenceInfo> Fetch(FileSearchResult result)
        {
            var results = new List<AssemblyReferenceInfo>();

            foreach(var relativePath in result.RelativeFilePaths)
            {
                var fullPath = Path.Combine(result.BaseDirectory, relativePath);
                var assembly = Assembly.LoadFrom(fullPath);

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
    }

    public class AssemblyReferenceInfo
    {
        public string SourceAssemblyFullPath { get; set; }
        public string SourceAssemblyRelativePath { get; set; }

        public AssemblyName SourceAssemblyName { get; set; }
        public AssemblyName[] ReferencedAssemblies { get; set; }
    }
}
