// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Collections.Generic;

namespace DumpAsmRefs
{
    public class AsmRefResult
    {
        public AsmRefResult(InputCriteria inputCriteria, IEnumerable<SourceAssemblyInfo> sourceAssemblyInfos)
        {
            InputCriteria = inputCriteria ?? throw new ArgumentNullException(nameof(inputCriteria));

            if (sourceAssemblyInfos == null)
            {
                throw new ArgumentNullException(nameof(sourceAssemblyInfos));
            }
            SourceAssemblyInfos = new List<SourceAssemblyInfo>(sourceAssemblyInfos);
        }

        public InputCriteria InputCriteria { get; }

        public IEnumerable<SourceAssemblyInfo> SourceAssemblyInfos { get; }
    }
}
