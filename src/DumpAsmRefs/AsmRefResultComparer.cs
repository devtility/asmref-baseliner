// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Data;
using DumpAsmRefs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DumpAsmRefs
{
    public class AsmRefResultComparer : IResultComparer
    {
        public bool AreSame(AsmRefResult first, AsmRefResult second, ComparisonOptions options)
        {
            if (!AreSame(first.InputCriteria, second.InputCriteria))
            {
                return false;
            }

            if (first.SourceAssemblyInfos.Count() != second.SourceAssemblyInfos.Count())
            {
                return false;
            }

            var sourceAssemblyNames = first.SourceAssemblyInfos
                .Select(x => AssemblyIdentifier.Parse(x.AssemblyName).Name)
                .ToArray();

            var asmNameToAsmRefInfoMap = second.SourceAssemblyInfos.ToDictionary(
                    x => AssemblyIdentifier.Parse(x.AssemblyName).Name,
                    x => x);

            foreach(var item in first.SourceAssemblyInfos)
            {
                // We need to find a matching AsmRefInfo, based solely on the assembly name
                // i.e. ignoring the version, public key etc
                var itemAsmInfo = AssemblyIdentifier.Parse(item.AssemblyName);

                var match = asmNameToAsmRefInfoMap
                    .FirstOrDefault(x => AreStringsSame(x.Key, itemAsmInfo.Name))
                    .Value;
                if (match == null) { return false; }

                // Now check the whole reference, taking into account the version compatibility level
                if (!IsSameSourceAssemblyInfo(item, match, options, sourceAssemblyNames))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool AreSame(InputCriteria input1, InputCriteria input2)
        {
            // Base directory and relative paths are ignored for the purposes
            // of this comparison.
            // The base directory is absolute so it can vary from machine to machine.
            return AreListsSame(input1.IncludePatterns, input2.IncludePatterns)
                && AreListsSame(input1.ExcludePatterns, input2.ExcludePatterns);
        }

        internal static bool IsSameSourceAssemblyInfo(SourceAssemblyInfo ref1, SourceAssemblyInfo ref2, ComparisonOptions options,
            IEnumerable<string> sourceAssemblyNames)
        {
            // SourceAssemblyFullPath is ignored for the purposes of this comparison
            // (it's absolute so can vary from machine to machine)

            // First check the simple strings
            if (!(AreStringsSame(ref1.RelativePath, ref2.RelativePath)
                && AreStringsSame(ref1.LoadException, ref2.LoadException)))
            {
                return false;
            }

            // Then check the source assembly name, taking into account version compatibility level
            var firstAsmInfo = AssemblyIdentifier.Parse(ref1.AssemblyName);
            var secondAsmInfo = AssemblyIdentifier.Parse(ref2.AssemblyName);
            if (!SourceAssembliesMatch(firstAsmInfo, secondAsmInfo, options))
            {
                return false;
            }

            // Finally, check the referenced assemblies, again taking into account version assembly compatibility level
            var firstAsmRefs = ref1.ReferencedAssemblies?.Select(AssemblyIdentifier.Parse) ?? Enumerable.Empty<AssemblyIdentifier>();
            var secondAsmRefs = ref2.ReferencedAssemblies?.Select(AssemblyIdentifier.Parse) ?? Enumerable.Empty<AssemblyIdentifier>();

            if (firstAsmRefs.Count() != secondAsmRefs.Count())
            {
                return false;
            }

            foreach (var firstItem in firstAsmRefs)
            {
                // Find the corresponding assembly by name
                var secondByName = secondAsmRefs.FirstOrDefault(x => AreStringsSame(firstItem.Name, x.Name));
                if (secondByName == null)
                {
                    return false;
                }

                var isSourceAssembly = sourceAssemblyNames.Contains(firstItem.Name);
                if (isSourceAssembly)
                {
                    if (!SourceAssembliesMatch(firstItem, secondByName, options))
                    {
                        return false;
                    }
                }
                else if (!NonSourceAssembliesMatch(firstItem, secondByName, options))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool NonSourceAssembliesMatch(AssemblyIdentifier first, AssemblyIdentifier second, ComparisonOptions options)
            => AreStringsSame(first.Name, second.Name)
                && AreStringsSame(first.CultureName, second.CultureName)
                && AreStringsSame(first.PublicKeyToken, second.PublicKeyToken)
                && VersionComparer.AreVersionsEqual(first.Version, second.Version, options.VersionCompatibility);

        internal static bool SourceAssembliesMatch(AssemblyIdentifier first, AssemblyIdentifier second, ComparisonOptions options)
            => AreStringsSame(first.Name, second.Name)
                && AreStringsSame(first.CultureName, second.CultureName)
                && (options.IgnoreSourcePublicKeyToken || AreStringsSame(first.PublicKeyToken, second.PublicKeyToken))
                && VersionComparer.AreVersionsEqual(first.Version, second.Version, options.VersionCompatibility);

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
