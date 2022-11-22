// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchRunnerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandlineBatcher.Diagnostics;
using CommandlineBatcher.Internal;
using FluentAssertions;
using Moq;
using Sundew.Base.Text;
using Xunit;

public class BatchRunnerTests
{
    private readonly IProcessRunner processRunner;
    private readonly IFileSystem fileSystem;
    private readonly BatchRunner testee;

    public BatchRunnerTests()
    {
        this.processRunner = New.Mock<IProcessRunner>();
        this.fileSystem = New.Mock<IFileSystem>();
        this.testee = new BatchRunner(this.processRunner, this.fileSystem, new ConditionEvaluator(New.Mock<IConditionEvaluatorReporter>()), New.Mock<IBatchRunnerReporter>());
        this.fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
    }

    [Fact]
    public async Task RunAsync_Then_ResultShouldContainExpectedProcesses()
    {
        var expected = new[] { MockTag("1.0.1", "Sundew.CommandLine"), MockPush("1.0.1", "Sundew.CommandLine"), MockTag("2.0.1", "Sundew.Base"), MockPush("2.0.1", "Sundew.Base") };
        processRunner.Setup(x => x.Run(It.IsAny<ProcessStartInfo>())).Returns<ProcessStartInfo>(
            psi =>
            {
                var process = New.Mock<IProcess>();
                process.SetupGet(x => x.StandardOutput).Returns(new StreamReader(new MemoryStream(Array.Empty<byte>())));
                process.SetupGet(x => x.StartInfo).Returns(psi);
                return process;
            });

        var result = await testee.RunAsync(
            new BatchArguments(
                new List<Command> { new("git", @"tag {0}-{1} -a -m ""Released {1} {0}"""), new("git", "push {0}-{1}") },
                BatchSeparation.CommandLine,
                null,
                new List<Values> { new("1.0.1", "Sundew.CommandLine"), new("2.0.1", "Sundew.Base") }));

        result.Should().Equal(expected, (p1, p2) => p1.StartInfo.FileName == p2.StartInfo.FileName && p1.StartInfo.Arguments == p2.StartInfo.Arguments);
    }

    [Fact]
    public async Task RunAsync_When_OutputtingToFile_Then_ExpectedTextShouldBeAppended()
    {
        processRunner.Setup(x => x.Run(It.IsAny<ProcessStartInfo>())).Returns<ProcessStartInfo>(
            psi =>
            {
                var process = New.Mock<IProcess>();
                process.SetupGet(x => x.StandardOutput).Returns(new StreamReader(new MemoryStream(Array.Empty<byte>())));
                process.SetupGet(x => x.StartInfo).Returns(psi);
                return process;
            });

        var result = await testee.RunAsync(
            new BatchArguments(
                new List<Command> { new(">> file.txt", @"Version={0}, Package={1} ""Package={1} Version={0}""{NL}") },
                BatchSeparation.CommandLine,
                null,
                new List<Values> { new("1.0.1", "Sundew.CommandLine"), new("2.0.1", "Sundew.Base") },
                fileEncoding: "utf8"));

        const string ExpectedFile = "file.txt";
        this.fileSystem.Verify(x => x.AppendAllText(ExpectedFile, $"Version=1.0.1, Package=Sundew.CommandLine \"Package=Sundew.CommandLine Version=1.0.1\"{Strings.NewLine}", Encoding.UTF8), Times.Once);
        this.fileSystem.Verify(x => x.AppendAllText(ExpectedFile, $"Version=2.0.1, Package=Sundew.Base \"Package=Sundew.Base Version=2.0.1\"{Strings.NewLine}", Encoding.UTF8), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenUsingBatchFiles_Then_ResultShouldContainExpectedProcesses()
    {
        var anyBatchFile = @"c:\temp\AnyFiles.txt";
        var expected = new[] { MockTag("1.0.1", "Sundew.CommandLine"), MockPush("1.0.1", "Sundew.CommandLine"), MockTag("2.0.1", "Sundew.Base"), MockPush("2.0.1", "Sundew.Base") };
        this.fileSystem.Setup(x => x.ReadAllText(anyBatchFile)).Returns(@"1.0.1|Sundew.CommandLine
2.0.1|Sundew.Base
");
        processRunner.Setup(x => x.Run(It.IsAny<ProcessStartInfo>())).Returns<ProcessStartInfo>(
            psi =>
            {
                var process = New.Mock<IProcess>();
                process.SetupGet(x => x.StandardOutput).Returns(new StreamReader(new MemoryStream(Array.Empty<byte>())));
                process.SetupGet(x => x.StartInfo).Returns(psi);
                return process;
            });

        var result = await testee.RunAsync(
            new BatchArguments(
                new List<Command> { new("git", @"tag {0}-{1} -a -m ""Released {1} {0}"""), new("git", "push {0}-{1}") },
                BatchSeparation.NewLine,
                "|",
                null,
                new List<string> { anyBatchFile }));

        result.Should().Equal(expected, (p1, p2) => p1.StartInfo.FileName == p2.StartInfo.FileName && p1.StartInfo.Arguments == p2.StartInfo.Arguments);
    }

    private static IProcess MockTag(string arg1, string arg2)
    {
        var process = New.Mock<IProcess>();
        process.SetupGet(x => x.StartInfo).Returns(new ProcessStartInfo("git", $@"tag {arg1}-{arg2} -a -m ""Released {arg2} {arg1}"""));
        return process;
    }

    private static IProcess MockPush(string arg1, string arg2)
    {
        var process = New.Mock<IProcess>();
        process.SetupGet(x => x.StartInfo).Returns(new ProcessStartInfo("git", $@"push {arg1}-{arg2}"));
        return process;
    }
}