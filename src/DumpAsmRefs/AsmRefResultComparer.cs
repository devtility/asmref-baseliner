// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public class AsmRefResultComparer
    {

        internal static bool AreSame(InputCriteria input1, InputCriteria input2)
        {
            // Base directory and relative paths are ignored for the purposes
            // of this comparison.
            // The base directory is absolute so it can vary from machine to machine.
            return AreListsSame(input1.IncludePatterns, input2.IncludePatterns)
                && AreListsSame(input1.ExcludePatterns, input2.ExcludePatterns);
        }

        internal static bool AreSame(AssemblyReferenceInfo ref1, AssemblyReferenceInfo ref2)
        {
            // SourceAssemblyFullPath is ignored for the purposes of this comparison (it's
            // absolute so can vary from machine to machine)
            return AreStringsSame(ref1.SourceAssemblyName, ref2.SourceAssemblyName)
                && AreStringsSame(ref1.SourceAssemblyRelativePath, ref2.SourceAssemblyRelativePath)
                && AreStringsSame(ref1.LoadException, ref2.LoadException)
                && AreListsSame(ref1.ReferencedAssemblies, ref2.ReferencedAssemblies);
        }

        internal static bool AreListsSame(IEnumerable<string> list1, IEnumerable<string> list2)
        {
            // Strict case-based comparison
            // Order is not important
            // A null list == an empty list

            var arr1 = list1?.ToArray() ?? Array.Empty<string>();
            var arr2 = list2?.ToArray() ?? Array.Empty<string>();

            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            return arr1.All(x => arr2.Contains(x, StringComparer.Ordinal));
        }

        private static bool AreStringsSame(string s1, string s2)
            => string.Equals(s1, s2, StringComparison.Ordinal);
    }
}
