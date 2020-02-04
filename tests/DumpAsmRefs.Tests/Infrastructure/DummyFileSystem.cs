using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;

namespace DumpAsmRefs.Tests.Infrastructure
{
    internal class DummyFileSystem : IFileSystem
    {
        private readonly Dictionary<string, string> fileToPathMap = new Dictionary<string, string>();

        public void AddFile(string path, string contents)
        {
            fileToPathMap[path] = contents;
        }

        #region IFileSystem implementation

        bool IFileSystem.FileExists(string path)
        {
            return fileToPathMap.ContainsKey(path);
        }

        string[] IFileSystem.ReadAllLines(string path)
        {
            return ((IFileSystem)this).ReadAllText(path).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        }

        string IFileSystem.ReadAllText(string path)
        {
            return fileToPathMap[path];
        }

        void IFileSystem.WriteAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        #endregion IFileSystem implementation
    }
}
