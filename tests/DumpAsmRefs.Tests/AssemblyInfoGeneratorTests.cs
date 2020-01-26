// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class AssemblyInfoGeneratorTests
    {
        [Fact]
        public void SystemAssembly_NoRefs()
        {
            //
            var testSubject = new TestableAssemblyInfoGenerator();

            var objectAssembly = typeof(object).Assembly;
            var expectedName = objectAssembly.GetName();
            testSubject.FilePathToAssemblyMap = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase)
            {
                { "c:\\foo\\system.dll", objectAssembly }
            };

            var result = testSubject.Fetch("c:\\foo\\", new string[] { "system.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            result[0].SourceAssemblyFullPath.Should().Be("c:\\foo\\system.dll");
            result[0].SourceAssemblyName.FullName.Should().Be(objectAssembly.GetName().FullName);

            result[0].ReferencedAssemblies.Should().NotBeNull();
            result[0].ReferencedAssemblies.Should().BeEmpty();
        }

        [Fact]
        public void SingleSourceAssembly_ReferencingMultipleAssemblies()
        {
            var testSubject = new TestableAssemblyInfoGenerator();

            var typesInRefAssemblies = new[] {
                typeof(object),
                typeof(System.Collections.CollectionBase),
                typeof(Xunit.ClassDataAttribute) };

            var asm = AssemblyCreator.CreateAssembly("foo", typesInRefAssemblies);

            testSubject.FilePathToAssemblyMap = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase)
            {
                { "c:\\foo.dll", asm }
            };

            var result = testSubject.Fetch("c:\\", new string[] { "foo.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            result[0].SourceAssemblyFullPath.Should().Be("c:\\foo.dll");
            result[0].SourceAssemblyName.Name.Should().Contain("foo");
            result[0].SourceAssemblyName.ToString().Should().Contain("Culture=neutral, PublicKeyToken=null");

            CheckReferencedAssemblies(result[0], typesInRefAssemblies);
        }

        [Fact]
        public void MultipleSourceAssemblies()
        {
            var asm1 = AssemblyCreator.CreateAssembly("asm1", typeof(object));
            var asm2 = AssemblyCreator.CreateAssembly("asm2", typeof(object), typeof(Xunit.ClassDataAttribute));

            var testSubject = new TestableAssemblyInfoGenerator();
            testSubject.FilePathToAssemblyMap = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase)
            {
                { "c:\\asm1.dll", asm1 },
                { "c:\\asm2.dll", asm2 }
            };

            var result = testSubject.Fetch("c:\\", new string[] { "asm1.dll", "asm2.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(2);

            result[0].SourceAssemblyFullPath.Should().Be("c:\\asm1.dll");
            result[1].SourceAssemblyFullPath.Should().Be("c:\\asm2.dll");

            CheckReferencedAssemblies(result[0], typeof(object));
            CheckReferencedAssemblies(result[1], typeof(object), typeof(Xunit.ClassDataAttribute));
        }

        private static void CheckReferencedAssemblies(AssemblyReferenceInfo result, params Type[] typesInRefAssemblies)
        {
            var refAsmFullNames = typesInRefAssemblies.Select(t => t.Assembly.GetName().FullName)
                .Distinct()
                .ToArray();

            result.ReferencedAssemblies.Select(ra => ra.FullName)
                .Should()
                .BeEquivalentTo(refAsmFullNames);


        }

        private class TestableAssemblyInfoGenerator : AssemblyInfoGenerator
        {
            public IDictionary<string, Assembly> FilePathToAssemblyMap { get; set; }

            protected override Assembly LoadAssembly(string fullFilePath)
            {
                if (FilePathToAssemblyMap.TryGetValue(fullFilePath, out var assembly))
                {
                    return assembly;
                }

                return null;
            }
        }
    }
}
