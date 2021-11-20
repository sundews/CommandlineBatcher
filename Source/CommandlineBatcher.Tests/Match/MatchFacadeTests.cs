// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchFacadeTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Tests.Match;

using System.Collections.Generic;
using System.Threading.Tasks;
using CommandlineBatcher.Internal;
using CommandlineBatcher.Match;
using FluentAssertions;
using Moq;
using Sundew.Base.Collections;
using Xunit;

public class MatchFacadeTests
{
    private const string DefaultInput = "1";
    private readonly MatchFacade testee;
    private readonly IInputter inputter;
    private readonly IOutputter outputter;
    private readonly IMatchReporter matchReporter;
    private readonly IFileSystem fileSystem;

    public MatchFacadeTests()
    {
        this.inputter = New.Mock<IInputter>();
        this.outputter = New.Mock<IOutputter>();
        this.matchReporter = New.Mock<IMatchReporter>();
        this.fileSystem = New.Mock<IFileSystem>();
        this.testee = new MatchFacade(this.inputter, this.outputter, this.fileSystem, matchReporter);
    }

    [Theory]
    [InlineData("main", new[] { "::set-output name=stage::production", "::set-output name=buildConfiguration::Release" })]
    [InlineData("release/1.2.0", new[] { "::set-output name=stage::ci", "::set-output name=buildConfiguration::Debug", "::set-output name=dev-package-source-if-set:: -s https://www.myget.org/F/sundew-dev/api/v3/index.json" })]
    [InlineData("feature/MyFeature", new[] { "::set-output name=stage::development", "::set-output name=buildConfiguration::Debug", "::set-output name=Postfix::MyFeature", "::set-output name=dev-package-source-if-set:: -s https://www.myget.org/F/sundew-dev/api/v3/index.json" })]
    [InlineData("invalid", new string[0])]
    public async Task MatchAsync_When_UsingInputAndFormatWithSpaces_Then_OutputterShouldBeCalledWithExpectedOutputs(string input, string[] expectedOutputs)
    {
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
        var patterns = new List<string>
        {
            @"(?:master|main).*                                          => stage=production|buildConfiguration=Release",
            @"release/.+                                                 => stage=ci|buildConfiguration=Debug|dev-package-source-if-set= -s https://www.myget.org/F/sundew-dev/api/v3/index.json",
            @"(?:develop.*|feature/(?<Postfix>.+)|bugfix/(?<Postfix>.+)) => stage=development|buildConfiguration=Debug|Postfix={Postfix}|dev-package-source-if-set= -s https://www.myget.org/F/sundew-dev/api/v3/index.json"
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "::set-output name={0}::{1}", null, '='));

        result.Should().Be(0);
        expectedOutputs.ForEach(x => this.outputter.Verify(outputter => outputter.OutputAsync(x), Times.Once));
        this.outputter.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("main", new[] { "::set-output name=stage::production", "::set-output name=buildConfiguration::Release" })]
    [InlineData("release/1.2.0", new[] { "::set-output name=stage::ci", "::set-output name=buildConfiguration::Debug", "::set-output name=dev-package-source-if-set:: -s https://www.myget.org/F/sundew-dev/api/v3/index.json" })]
    [InlineData("feature/MyFeature", new[] { "::set-output name=stage::development", "::set-output name=buildConfiguration::Debug", "::set-output name=Postfix::MyFeature", "::set-output name=dev-package-source-if-set:: -s https://www.myget.org/F/sundew-dev/api/v3/index.json" })]
    [InlineData("invalid", new string[0])]
    public async Task MatchAsync_When_UsingInputAndFormat_Then_OutputterShouldBeCalledWithExpectedOutputs(string input, string[] expectedOutputs)
    {
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
        var patterns = new List<string>
        {
            @"(?:master|main).*=> stage=production|buildConfiguration=Release",
            @"release/.+ => stage=ci|buildConfiguration=Debug|dev-package-source-if-set= -s https://www.myget.org/F/sundew-dev/api/v3/index.json",
            @"(?:develop.*|feature/(?<Postfix>.+)|bugfix/(?<Postfix>.+)) => stage=development|buildConfiguration=Debug|Postfix={Postfix}|dev-package-source-if-set= -s https://www.myget.org/F/sundew-dev/api/v3/index.json"
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "::set-output name={0}::{1}", null, '='));

        result.Should().Be(0);
        expectedOutputs.ForEach(x => this.outputter.Verify(outputter => outputter.OutputAsync(x), Times.Once));
        this.outputter.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("1", new[] { "1+3=4" })]
    [InlineData("2", new[] { "2+3=5" })]
    [InlineData("3", new[] { "3+3=6" })]
    public async Task MatchAsync_When_UsingInputAndSimpleMapAndFormat_Then_OutputterShouldBeCalledWithExpectedOutputs(string input, string[] expectedOutputs)
    {
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
        var patterns = new List<string>
        {
            @"1 => 1=4",
            @"2 => 2=5",
            @"3 => 3=6"
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "{0}+3={1}", null, '='));

        result.Should().Be(0);
        expectedOutputs.ForEach(x => this.outputter.Verify(outputter => outputter.OutputAsync(x), Times.Once));
        this.outputter.VerifyNoOtherCalls();
    }


    [Theory]
    [InlineData(@"'1'  => 4 dogs,5 cats,6 frogs", DefaultInput, "I have 4 dogs, 5 cats and 6 frogs at home")]
    [InlineData(@"'1'  =>4 dogs,5 cats,6 frogs", DefaultInput, "I have 4 dogs, 5 cats and 6 frogs at home")]
    [InlineData(@"'1 ' =>4 dogs,5 cats,6 frogs", "1 ", "I have 4 dogs, 5 cats and 6 frogs at home")]
    public async Task MatchAsync_When_UsingInputAndSimpleMap_Then_OutputterShouldBeCalledWithExpectedOutput(string pattern, string input, string expectedOutput)
    {
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
        var patterns = new List<string>
        {
            pattern,
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "I have {0}, {1} and {2} at home"));

        result.Should().Be(0);
        this.outputter.Verify(outputter => outputter.OutputAsync(expectedOutput), Times.Once);
        this.outputter.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(@"'1''  => Quoted ,with qoute at the end", "1'", "Quoted result with qoute at the end")]
    [InlineData(@"'2  =>,with qoute at the beginning", "'2", "result with qoute at the beginning")]
    [InlineData(@"3' =>,with qoute at the end", "3'", "result with qoute at the end")]
    public async Task MatchAsync_When_UsingQuote_Then_OutputterShouldBeCalledWithExpectedOutput(string pattern, string input, string expectedOutput)
    {
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(input);
        var patterns = new List<string>
        {
            pattern,
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, input, false, "{0}result {1}"));

        result.Should().Be(0);
        this.outputter.Verify(outputter => outputter.OutputAsync(expectedOutput), Times.Once);
        this.outputter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MatchAsync_When_UsingInputAndSimpleMapAndUsingMergeFormat_Then_OutputterShouldBeCalledWithExpectedOutputs()
    {
        const string ExpectedOutput = "I have 4 dogs, 5 cats, 6 frogs at home";
        this.inputter.Setup(x => x.GetInputAsync()).ReturnsAsync(DefaultInput);
        var patterns = new List<string>
        {
            @"1 => 4 dogs|5 cats|6 frogs",
        };

        var result = await this.testee.MatchAsync(new MatchVerb(patterns, DefaultInput, false, null, mergeDelimiter: ", ", mergeFormat: "I have {0} at home"));

        result.Should().Be(0);
        this.outputter.Verify(outputter => outputter.OutputAsync(ExpectedOutput), Times.Once);
        this.outputter.VerifyNoOtherCalls();
    }
}