// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchFacadeTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Tests.Match
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CommandlineBatcher.Match;
    using Moq;
    using Sundew.Base.Collections;
    using Xunit;

    public class MatchFacadeTests
    {
        private readonly MatchFacade testee;
        private readonly IInputter inputter;
        private readonly IOutputter outputter;
        private readonly IMatchReporter matchReporter;

        public MatchFacadeTests()
        {
            this.inputter = New.Mock<IInputter>();
            this.outputter = New.Mock<IOutputter>();
            this.matchReporter = New.Mock<IMatchReporter>();
            this.testee = new MatchFacade(inputter, outputter, matchReporter);
        }

        [Theory]
        [InlineData("main", new[] { "::set-output name=stage::production", "::set-output name=buildConfiguration::Release" })]
        [InlineData("release/1.2.0", new[] { "::set-output name=stage::ci", "::set-output name=buildConfiguration::Debug" })]
        [InlineData("feature/MyFeature", new[] { "::set-output name=stage::development", "::set-output name=buildConfiguration::Debug", "::set-output name=Postfix::MyFeature" })]
        [InlineData("invalid", new string[0])]
        public async Task MatchAsync_When_UsingInput_Then_OutputterShouldBeCalledWithExpectedOutputs(string input, string[] expectedOutputs)
        {
            this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
            var patterns = new List<string>
            {
                @"(?:master|main).* => stage=production|buildConfiguration=Release",
                @"release/.+ => stage=ci|buildConfiguration=Debug",
                @"(?:develop.*|feature/(?<Postfix>.+)|bugfix/(?<Postfix>.+)) => stage=development|buildConfiguration=Debug|Postfix={Postfix}"
            };

            await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "::set-output name={0}::{1}", null, '='));

            expectedOutputs.ForEach(x => this.outputter.Verify(outputter => outputter.OutputAsync(x), Times.Once));
            this.outputter.VerifyNoOtherCalls();
        }
    }
}