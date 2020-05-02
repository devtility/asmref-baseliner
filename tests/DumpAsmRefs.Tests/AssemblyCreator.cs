// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace DumpAsmRefs.Tests
{
    internal static class AssemblyCreator
    {
        public static Assembly CreateAssembly(ITestOutputHelper logger, string assemblyName, params Type[] nonStaticReferencedTypes)
        {
            var code = CreateCode(nonStaticReferencedTypes);
            logger.WriteLine($"Test assembly code for {assemblyName}:");
            logger.WriteLine(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var refs = nonStaticReferencedTypes.Select(CreateMetadataReference);

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug);

            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, refs, compilationOptions);
            
            var stream = new MemoryStream();
            var result = compilation.Emit(stream);

            if (!result.Success)
            {
                foreach(var diag in result.Diagnostics)
                {
                    logger.WriteLine(diag.ToString());
                }
                throw new InvalidOperationException("Test setup error: error compiling test assembly");
            }

            var data = stream.GetBuffer();

            var assembly = Assembly.Load(data);
            return assembly;
        }

        private static MetadataReference CreateMetadataReference(Type typeInAssembly)
            =>MetadataReference.CreateFromFile(typeInAssembly.Assembly.Location);

        private static string CreateCode(IEnumerable<Type> referencedTypes)
        {
            const string codeTemplate = @"
namespace Dummy
{
  class DummmyClass
  {
    object Method1(***) { return this; }
  }
}
";

            var index = 0;
            var parameters = referencedTypes.Select(rt => rt.FullName + " x" + index++);
            var paramList = string.Join("," + Environment.NewLine, parameters);

            var code = codeTemplate.Replace("***", paramList);
            return code;
        }
    }
}
