// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.Interfaces
{
    internal interface IReportBuilder
    {
        string Generate(AsmRefResult asmRefResult);
    }
}
