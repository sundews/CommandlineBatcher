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
        private const string PatternRegexFormat = @"^(?<MatchRegex>.+?)\s?=\>\s?(?<Values>[^\{0}]+)(?:\{0}(?<Values>[^\{0}]+))*";
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

                var patternsRegex = new Regex(string.Format(PatternRegexFormat, matchVerb.BatchSeparator), RegexOptions.ExplicitCapture);
                var patternMatches = matchVerb.Patterns.Select(x =>
                {
                    var pattern = patternsRegex.Match(x);
                    var patternText = pattern.Groups[MatchRegex].Value;
                    return (regex: new Regex(patternText), values: pattern.Groups[Values].Captures.Select(capture => capture.Value).ToArray(), pattern: patternText);
                });

                var shouldMerge = matchVerb.MergeFormat != null || matchVerb.MergeDelimiter != null;
                var matchAndValues = patternMatches.Select(tuple => (match: tuple.regex.Match(input), tuple.regex, tuple.values, tuple.pattern)).FirstOrDefault(tuple => tuple.match.Success);
                if (matchAndValues != default)
                {
                    this.matchReporter.Report($"The input: {input} matched: {matchAndValues.pattern}");
                    if (matchAndValues.values.Length == 0)
                    {
                        return 0;
                    }

                    var stringBuilder = new StringBuilder();
                    Formatter.AppendFormat(stringBuilder, matchVerb.Format, matchAndValues.values[0], matchAndValues.regex, matchAndValues.match.Groups, matchVerb.BatchValueSeparator);
                    if (string.IsNullOrEmpty(matchVerb.MergeFormat))
                    {
                        await this.outputter.OutputAsync(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }

                    for (var index = 1; index < matchAndValues.values.Length; index++)
                    {
                        var value = matchAndValues.values[index];
                        if (matchVerb.MergeDelimiter != null)
                        {
                            stringBuilder.Append(matchVerb.MergeDelimiter);
                        }

                        Formatter.AppendFormat(stringBuilder, matchVerb.Format, value, matchAndValues.regex, matchAndValues.match.Groups, matchVerb.BatchValueSeparator);
                        if (!shouldMerge)
                        {
                            await this.outputter.OutputAsync(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                    }

                    if (matchVerb.MergeFormat != null)
                    {
                        await this.outputter.OutputAsync(string.Format(matchVerb.MergeFormat, stringBuilder));
                        stringBuilder.Clear();
                    }
                }
                else
                {
                    this.matchReporter.Report($"The input: {input} did not match any pattern.");
                }

                return 0;
            }
            catch (Exception e)
            {
                this.matchReporter.Exception(e);
                return -1;
            }
        }
    }
}