// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.IO;

namespace DumpAsmRefs
{
    internal class FileSystemAbstraction : IFileSystem
    {
        bool IFileSystem.FileExists(string path) => File.Exists(path);

        void IFileSystem.WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
        
        string IFileSystem.ReadAllText(string path) => File.ReadAllText(path);

        string[] IFileSystem.ReadAllLines(string path) => File.ReadAllLines(path);
    }
}
