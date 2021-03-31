// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionEvaluatorTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Tests.Internal
{
    using CommandlineBatcher.Internal;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class ConditionEvaluatorTests
    {
        [Theory]
        [InlineData("lhs=={0}", "lhs", true)]
        [InlineData("lhs=={0}", "rhs", false)]
        [InlineData("lhs == {0}", "lhs", true)]
        [InlineData("lhs == {0}", "rhs", false)]
        [InlineData("lhs =={0}", "lhs", true)]
        [InlineData("lhs =={0}", "rhs", false)]
        [InlineData("lhs with space =={0}", "lhs with space", true)]
        [InlineData("lhs with space =={0}", "rhs with space", false)]
        [InlineData("lhs with space !={0}", "lhs with space", false)]
        [InlineData("lhs with space     =={0}", "lhs with space    ", true)]
        [InlineData("CI:LHS=={0}", "lhs", true)]
        [InlineData("CI:LHS!={0}", "lhs", false)]
        [InlineData("CI:I have LHS in me><{0}", "lhs", true)]
        [InlineData("CI:I have LHS in me >< {0}", "lhs", true)]
        public void Evaluate_Then_ResultShouldBeExpectedResult(string condition, string value, bool expectedResult)
        {
            var testee = new ConditionEvaluator(New.Mock<IConditionEvaluatorReporter>());

            var result = testee.Evaluate(condition, Values.From(value, "|"));

            result.Should().Be(expectedResult);
        }
    }
}