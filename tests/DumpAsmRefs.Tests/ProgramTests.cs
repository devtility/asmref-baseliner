// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using DumpAsmRefs.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DumpAsmRefs.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ParsingError()
        {
            var result = Program.Main(Array.Empty<string>());
            result.Should().Be((int)ExitCodes.ParsingError);
        }

        [Fact]
        public void Execute_EndToEnd_Success()
        {
            var logger = new TestLogger();
            var fileLocator = new Mock<IFileLocator>();
            var asmInfoGenerator = new Mock<IAssemblyInfoGenerator>();
            var reportBuilder = new Mock<IReportBuilder>();
            var fileSystem = new Mock<IFileSystem>();

            var userArgs = new UserArguments("c:\\root", new string[] { "include1" },
                new string[] { "exclude1" }, "outfile.txt", Verbosity.Detailed);

            var matchingFiles = new string[] { "file1.txt", "\\sub\\file2.txt" };
            fileLocator.Setup(x => x.Search(userArgs.RootDirectory,
                userArgs.IncludePatterns, userArgs.ExcludePatterns)).Returns(matchingFiles);

            var asmRefInfo = Array.Empty<SourceAssemblyInfo>();
            asmInfoGenerator.Setup(x => x.Fetch(userArgs.RootDirectory, matchingFiles))
                .Returns(asmRefInfo);

            // Execute
            var result = Program.Execute(userArgs, fileLocator.Object, asmInfoGenerator.Object,
                reportBuilder.Object,  fileSystem.Object, logger);

            // Checks
            result.Should().Be((int)ExitCodes.Success);

            fileLocator.Verify(x => x.Search(userArgs.RootDirectory, userArgs.IncludePatterns, userArgs.ExcludePatterns),
                Times.Once);

            asmInfoGenerator.Verify(x => x.Fetch(userArgs.RootDirectory, matchingFiles),
                Times.Once);

            reportBuilder.Verify(x => x.Generate(It.IsAny<AsmRefResult>()), Times.Once);

            fileSystem.Verify(x => x.WriteAllText(userArgs.OutputFileFullPath, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Execute_NoMatchingFiles()
        {
            var logger = new TestLogger();
            var fileLocator = new Mock<IFileLocator>();
            var asmInfoGenerator = new Mock<IAssemblyInfoGenerator>();
            var reportBuilder = new Mock<IReportBuilder>();
            var fileSystem = new Mock<IFileSystem>();

            var userArgs = new UserArguments("c:\\root", new string[] { "include1" },
                new string[] { "exclude1" }, "outfile.txt", Verbosity.Detailed);

            fileLocator.Setup(x => x.Search(userArgs.RootDirectory,
                userArgs.IncludePatterns, userArgs.ExcludePatterns)).Returns(Enumerable.Empty<string>());

            // Execute
            var result = Program.Execute(userArgs, fileLocator.Object, asmInfoGenerator.Object,
                reportBuilder.Object, fileSystem.Object, logger);

            // Checks
            result.Should().Be((int)ExitCodes.NoMatchingFiles);

            fileLocator.Verify(x => x.Search(userArgs.RootDirectory, userArgs.IncludePatterns, userArgs.ExcludePatterns),
                Times.Once);

            asmInfoGenerator.Verify(x => x.Fetch(It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>()), Times.Never);
                
            reportBuilder.Verify(x => x.Generate(It.IsAny<AsmRefResult>()), Times.Never);

            fileSystem.Verify(x => x.WriteAllText(userArgs.OutputFileFullPath, It.IsAny<string>()), Times.Never);
        }
    }
}
