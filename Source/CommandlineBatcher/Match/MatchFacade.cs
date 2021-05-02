// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class MatchFacade
    {
        private const string PatternRegexFormat = @"^(?<MatchRegex>(?:[^#\s])+)\s*=\>\s*(?<Values>[^{0}]+)(?:\{0}(?<Values>[^{0}]+))*";
        private const string MatchRegex = "MatchRegex";
        private const string Values = "Values";
        private readonly IInputter inputter;
        private readonly IOutputter outputter;
        private readonly IMatchReporter matchReporter;

        public MatchFacade(IInputter inputter, IOutputter outputter, IMatchReporter matchReporter)
        {
            this.inputter = inputter;
            this.outputter = outputter;
            this.matchReporter = matchReporter;
        }

        public async Task<int> MatchAsync(MatchVerb matchVerb)
        {
            try
            {
                var input = await this.inputter.GetInputAsync();

                var patternsRegex = new Regex(string.Format(PatternRegexFormat, matchVerb.TupleSeparator), RegexOptions.ExplicitCapture);
                var patternMatches = matchVerb.Patterns.Select(x =>
                {
                    var pattern = patternsRegex.Match(x);
                    return (regex: new Regex(pattern.Groups[MatchRegex].Value), values: pattern.Groups[Values].Captures.Select(capture => capture.Value));
                });

                var matchAndValues = patternMatches.Select(tuple => (match: tuple.regex.Match(input), tuple.regex, tuple.values)).FirstOrDefault(tuple => tuple.match.Success);
                if (matchAndValues != default)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var value in matchAndValues.values)
                    {
                        Formatter.AppendFormat(stringBuilder, matchVerb.Format, value, matchAndValues.regex, matchAndValues.match.Groups, matchVerb.TupleValueSeparator);
                        if (!matchVerb.Merge)
                        {
                            await this.outputter.OutputAsync(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                    }

                    if (matchVerb.Merge)
                    {
                        await this.outputter.OutputAsync(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                this.matchReporter.Exception(e);
                return - 1;
            }
        }
    }
}