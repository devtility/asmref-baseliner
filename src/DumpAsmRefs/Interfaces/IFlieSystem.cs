// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

namespace DumpAsmRefs.Interfaces
{
    // Abstraction over the file system for testing
    internal interface IFileSystem
    {
        bool FileExists(string path);

        void WriteAllFile(string path, string contents);

        string ReadAllText(string path);

        string[] ReadAllLines(string path);
    }
}
