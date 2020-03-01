// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Collections.Generic;

namespace DumpAsmRefs
{
    public class AsmRefResult
    {
        public AsmRefResult(InputCriteria inputCriteria, IEnumerable<AssemblyReferenceInfo> assemblyReferenceInfos)
        {
            InputCriteria = inputCriteria ?? throw new ArgumentOutOfRangeException(nameof(inputCriteria));

            if (assemblyReferenceInfos == null)
            {
                throw new ArgumentOutOfRangeException(nameof(assemblyReferenceInfos));
            }
            AssemblyReferenceInfos = new List<AssemblyReferenceInfo>(assemblyReferenceInfos);
        }

        public InputCriteria InputCriteria { get; }

        public IEnumerable<AssemblyReferenceInfo> AssemblyReferenceInfos { get; }
    }
}
