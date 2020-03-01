// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;

namespace DumpAsmRefs
{    
    public class AssemblyReferenceInfo
    {
        public string LoadException { get; set; }

        public string SourceAssemblyFullPath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Relative path")]
        public string SourceAssemblyRelativePath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Assembly")]
        public string SourceAssemblyName { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Referenced assemblies")]
        public IEnumerable<string> ReferencedAssemblies { get; set; }
    }
}
