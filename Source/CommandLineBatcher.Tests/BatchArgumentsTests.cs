namespace CommandLineBatcher.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Sundew.Base.Computation;
    using Sundew.CommandLine;
    using Xunit;

    public class BatchArgumentsTests
    {
        [Theory]
        [MemberData(nameof(Values))]
        public void Test1(Data data)
        {
            var testee = new CommandLineParser<int, int>();
            var arguments = testee.WithArguments(new BatchArguments(), arguments => Result.Success(0));

            testee.Parse(data.CommandLine);

            arguments.Values.Should().Equal(data.ExpectedResult, (a, e) => a.Arguments.SequenceEqual(e));
        }

        public static IEnumerable<object[]> Values()
        {
            yield return new[] { new Data("-c git -v 1.0.1|Sundew.CommandLine 2.0.1|Sundew.Base", new[] { new[] { "1.0.1", "Sundew.CommandLine" }, new[] { "2.0.1", "Sundew.Base" } }) };
        }

        public record Data(string CommandLine, string[][] ExpectedResult);
    }
}
