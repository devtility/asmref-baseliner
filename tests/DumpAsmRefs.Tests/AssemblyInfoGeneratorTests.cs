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
            var testSubject = new TestableAssemblyInfoGenerator();

            var objectAssembly = typeof(object).Assembly;
            testSubject.FilePathToAssemblyMap["c:\\foo\\system.dll"] = objectAssembly;

            var result = testSubject.Fetch("c:\\foo\\", new string[] { "system.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            result[0].FullPath.Should().Be("c:\\foo\\system.dll");
            result[0].AssemblyName.Should().Be(objectAssembly.GetName().FullName);

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

            testSubject.FilePathToAssemblyMap["c:\\foo.dll"] = asm;

            var result = testSubject.Fetch("c:\\", new string[] { "foo.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            result[0].FullPath.Should().Be("c:\\foo.dll");
            result[0].AssemblyName.Should().Contain("foo");
            result[0].AssemblyName.ToString().Should().Contain("Culture=neutral, PublicKeyToken=null");

            CheckReferencedAssemblies(result[0], typesInRefAssemblies);
        }

        [Fact]
        public void MultipleSourceAssemblies()
        {
            var asm1 = AssemblyCreator.CreateAssembly("asm1", typeof(object));
            var asm2 = AssemblyCreator.CreateAssembly("asm2", typeof(object), typeof(Xunit.ClassDataAttribute));

            var testSubject = new TestableAssemblyInfoGenerator();
            testSubject.FilePathToAssemblyMap["c:\\asm1.dll"] = asm1;
            testSubject.FilePathToAssemblyMap["c:\\asm2.dll"] = asm2;
               
            var result = testSubject.Fetch("c:\\", new string[] { "asm1.dll", "asm2.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(2);

            result[0].FullPath.Should().Be("c:\\asm1.dll");
            result[1].FullPath.Should().Be("c:\\asm2.dll");

            CheckReferencedAssemblies(result[0], typeof(object));
            CheckReferencedAssemblies(result[1], typeof(object), typeof(Xunit.ClassDataAttribute));
        }

        [Fact]
        public void ExceptionLoadingAssembly_Captured()
        {
            // Assembly load exceptions should be caught and should not prevent
            // later assemblies from being processed
            var testSubject = new TestableAssemblyInfoGenerator();

            // Set up unloadable assembly
            var exceptionFilePath = "c:\\invalid1.dll";
            var expectedException = new BadImageFormatException("foo");
            testSubject.FilePathToExceptionToThrowMap[exceptionFilePath] = expectedException;

            // Set up valid assemblies
            var validAsm1 = AssemblyCreator.CreateAssembly("valid1", typeof(System.Collections.CollectionBase));
            var validAsm2 = AssemblyCreator.CreateAssembly("valid2", typeof(object));

            testSubject.FilePathToAssemblyMap["c:\\valid1.dll"] = validAsm1;
            testSubject.FilePathToAssemblyMap["c:\\valid2.dll"] = validAsm2;

            var result = testSubject.Fetch("c:\\", new string[] { "valid1.dll", "invalid1.dll", "valid2.dll" });

            // Checks
            result.Should().NotBeNull();
            result.Count.Should().Be(3);

            result[0].FullPath.Should().Be("c:\\valid1.dll");
            CheckReferencedAssemblies(result[0], typeof(System.Collections.CollectionBase));

            result[1].FullPath.Should().Be("c:\\invalid1.dll");
            result[1].LoadException.Should().ContainAll("System.BadImageFormatException", UIStrings.InfoGenerator_NotADotNetAssembly);

            result[1].ReferencedAssemblies.Should().BeNull();

            result[2].FullPath.Should().Be("c:\\valid2.dll");
            CheckReferencedAssemblies(result[2], typeof(System.Collections.CollectionBase));
        }

        private static void CheckReferencedAssemblies(SourceAssemblyInfo result, params Type[] typesInRefAssemblies)
        {
            var refAsmFullNames = typesInRefAssemblies.Select(t => t.Assembly.GetName().FullName)
                .Distinct()
                .ToArray();

            result.LoadException.Should().BeNull();

            result.ReferencedAssemblies.Should().BeEquivalentTo(refAsmFullNames);
        }

        private class TestableAssemblyInfoGenerator : AssemblyInfoGenerator
        {
            public IDictionary<string, Assembly> FilePathToAssemblyMap { get; set; }
                = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

            public IDictionary<string, Exception> FilePathToExceptionToThrowMap { get; set; }
                = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);

            protected override Assembly LoadAssembly(string fullFilePath)
            {
                if (FilePathToExceptionToThrowMap.TryGetValue(fullFilePath, out var exception))
                {
                    throw exception;
                }

                if (FilePathToAssemblyMap.TryGetValue(fullFilePath, out var assembly))
                {
                    return assembly;
                }

                return null;
            }
        }
    }
}
