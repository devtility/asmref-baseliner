// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Reflection;
using System.Text;

namespace DumpAsmRefs.Data
{
    public class AssemblyInfo
    {
        private AssemblyInfo(string name, Version version, string cultureInfo, string publicKeyToken)
        {
            Name = name;
            Version = version;
            CultureName = cultureInfo;
            PublicKeyToken = publicKeyToken;
        }

        public string Name { get; }
        public Version Version { get; }
        public string CultureName { get; }
        public string PublicKeyToken { get; }

        public static AssemblyInfo Parse(string fullName)
        {
            var asmName = ParseAssemblyName(fullName);
            if (asmName == null)
            {
                return null;
            }

            var token = GetPublicKeyTokenAsString(asmName);
            return new AssemblyInfo(asmName.Name, asmName.Version, asmName.CultureName, token);
        }

        private static AssemblyName ParseAssemblyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            try
            {
                var asmName = new System.Reflection.AssemblyName(name);
                return asmName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetPublicKeyTokenAsString(AssemblyName assemblyName)
        {
            if (assemblyName == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            var pt = assemblyName.GetPublicKeyToken();
            for (int i = 0; i < pt.Length; i++)
            {
                sb.AppendFormat("{0:x2}", pt[i]);
            }
            var token = sb.ToString();
            return token;
        }
    }
}
