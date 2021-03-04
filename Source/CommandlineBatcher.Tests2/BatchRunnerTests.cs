// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchRunnerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using CommandlineBatcher.Diagnostics;
    using CommandlineBatcher.Internal;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class BatchRunnerTests
    {
        [Fact]
        public async Task RunAsync()
        {
            var processRunner = New.Mock<IProcessRunner>();
            var testee = new BatchRunner(processRunner, New.Mock<IFileSystem>(), New.Mock<IBatchRunnerReporter>());
            var expected = new[] { MockTag("1.0.1", "Sundew.CommandLine"), MockPush("1.0.1", "Sundew.CommandLine"), MockTag("2.0.1", "Sundew.Base"), MockPush("2.0.1", "Sundew.Base") };
            processRunner.Setup(x => x.Run(It.IsAny<ProcessStartInfo>())).Returns<ProcessStartInfo>(
                psi =>
                {
                    var process = New.Mock<IProcess>();
                    process.SetupGet(x => x.StandardOutput).Returns(new StreamReader(new MemoryStream(new byte[0])));
                    process.SetupGet(x => x.StartInfo).Returns(psi);
                    return process;
                });

            var result = await testee.RunAsync(
                new BatchArguments(
                    new List<Command> { new("git", @"tag {0}-{1} -a -m ""Released {1} {0}"""), new("git", "push {0}-{1}") },
                    null,
                    new List<Values> { new("1.0.1", "Sundew.CommandLine"), new("2.0.1", "Sundew.Base") }));

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
}