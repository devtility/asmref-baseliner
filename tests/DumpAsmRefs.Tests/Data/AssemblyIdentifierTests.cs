// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Data;
using FluentAssertions;
using System;
using Xunit;

namespace DumpAsmRefs.Tests.Data
{
    public class AssemblyIdentifierTests
    {
        [Fact]
        public void Parse_NullOrEmpty()
        {
            // Null/empty
            AssemblyIdentifier.Parse(null).Should().BeNull();
            AssemblyIdentifier.Parse("").Should().BeNull();
        }

        [Fact]
        public void Parse_NotStrongNamed()
        {
            var result = AssemblyIdentifier.Parse("DumpAsmRefs, Version=0.8.0.0, Culture=neutral, PublicKeyToken=null");

            result.Name.Should().Be("DumpAsmRefs");
            result.Version.Should().Be(new Version(0, 8, 0, 0));
            result.CultureName.Should().Be("");
            result.PublicKeyToken.Should().Be("");
        }

        [Fact]
        public void Parse_StrongNamed()
        {
            var result = AssemblyIdentifier.Parse("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            result.Name.Should().Be("System");
            result.Version.Should().Be(new Version(4, 0, 0, 0));
            result.CultureName.Should().Be("");
            result.PublicKeyToken.Should().Be("b77a5c561934e089");
        }
    }
}
