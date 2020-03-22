// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;

namespace DumpAsmRefs
{    
    /// <summary>
    /// Data class for a single analyzed assembly
    /// </summary>
    public class SourceAssemblyInfo
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "Assembly load exception")]
        public string LoadException { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public string FullPath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Relative path")]
        public string RelativePath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Assembly")]
        public string AssemblyName { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Referenced assemblies")]
        public IEnumerable<string> ReferencedAssemblies { get; set; }
    }
}
