namespace CommandlineBatcher.Tests;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sundew.Base;
using Sundew.CommandLine;
using Xunit;

public class BatchArgumentsTests
{
    [Theory]
    [MemberData(nameof(Values))]
    public void Parse_Then_ArgumentsShouldBeParsed(Data data)
    {
        var testee = new CommandLineParser<int, int>();
        var arguments = testee.WithArguments(new BatchArguments(), arguments => R.Success(0));

        testee.Parse(data.CommandLine);

        arguments.Batches.Should().Equal(data.ExpectedResult, (a, e) => a.Arguments.SequenceEqual(e));
    }

    public static IEnumerable<object[]> Values()
    {
        yield return new[] { new Data("-c git -bvs | -b 1.0.1|Sundew.CommandLine 2.0.1|Sundew.Base", new[] { new[] { "1.0.1", "Sundew.CommandLine" }, new[] { "2.0.1", "Sundew.Base" } }) };
    }

    public record Data(string CommandLine, string[][] ExpectedResult);
}