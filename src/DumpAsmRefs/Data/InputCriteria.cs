// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public class InputCriteria
    {
        // Required by the YamlDotNet deserializer
        public InputCriteria()
        {
        }

        public InputCriteria(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            BaseDirectory = baseDirectory;
            IncludePatterns = new List<string>(includePatterns ?? Enumerable.Empty<string>());
            ExcludePatterns = new List<string>(excludePatterns ?? Enumerable.Empty<string>());
        }

        [YamlDotNet.Serialization.YamlIgnore]
        public string BaseDirectory { get; set;  }

        [YamlDotNet.Serialization.YamlMember(Alias = "Include patterns")]
        public IEnumerable<string> IncludePatterns { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "Exclude patterns")]
        public IEnumerable<string> ExcludePatterns { get; set; }
    }
}
