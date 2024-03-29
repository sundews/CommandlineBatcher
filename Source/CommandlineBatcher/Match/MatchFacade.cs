﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using CommandlineBatcher.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class MatchFacade
{
    private const string PatternRegexFormat = @"^(?<StartQuote>\')?(?<MatchRegex>.+?)(?<StopQuote-StartQuote>\')?(?(StartQuote)(?!))\s*=\>\s?(?<Values>[^\{0}]*)(?:\{0}(?<Values>[^\{0}]+))*";
    private const string MatchRegex = "MatchRegex";
    private const string Values = "Values";
    private readonly IInputter inputter;
    private readonly IOutputter outputter;
    private readonly IFileSystem fileSystem;
    private readonly IMatchReporter matchReporter;

    public MatchFacade(IInputter inputter, IOutputter outputter, IFileSystem fileSystem, IMatchReporter matchReporter)
    {
        this.inputter = inputter;
        this.outputter = outputter;
        this.fileSystem = fileSystem;
        this.matchReporter = matchReporter;
    }

    public async Task<int> MatchAsync(MatchVerb matchVerb)
    {
        try
        {
            var inputs = await this.inputter.GetInputAsync();
            var stringBuilder = new StringBuilder();
            var shouldMerge = matchVerb.MergeFormat != null || matchVerb.MergeDelimiter != null;
            foreach (var input in inputs)
            {
                var patternsRegex = new Regex(string.Format(PatternRegexFormat, matchVerb.BatchSeparator),
                    RegexOptions.ExplicitCapture);
                var patterns = matchVerb.Patterns.Select(x =>
                {
                    var pattern = patternsRegex.Match(x);
                    var patternText = pattern.Groups[MatchRegex].Value;
                    return (regex: new Regex(patternText),
                        values: pattern.Groups[Values].Captures.Select(capture => capture.Value).ToArray(),
                        pattern: patternText);
                });

                var matchesWithValues = patterns
                    .Select(tuple => (match: tuple.regex.Match(input), tuple.regex, tuple.values, tuple.pattern))
                    .Where(tuple => tuple.match.Success).ToList();
                foreach (var matchAndValues in matchesWithValues)
                {
                    this.matchReporter.Report($"The input: {input} matched: {matchAndValues.pattern}");
                    if (matchAndValues.values.Length == 0)
                    {
                        continue;
                    }

                    var workingDirectory = string.IsNullOrEmpty(matchVerb.WorkingDirectory)
                        ? this.fileSystem.GetCurrentDirectory()
                        : Path.GetFullPath(matchVerb.WorkingDirectory);

                    if (stringBuilder.Length > 0 && matchVerb.MergeDelimiter != null)
                    {
                        stringBuilder.Append(matchVerb.MergeDelimiter);
                    }

                    stringBuilder.AppendFormatted(matchVerb.Format, matchAndValues.values[0],
                        matchAndValues.regex, matchAndValues.match.Groups, matchVerb.BatchValueSeparator,
                        workingDirectory);

                    if (string.IsNullOrEmpty(matchVerb.MergeFormat))
                    {
                        await this.outputter.OutputAsync(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }

                    for (var index = 1; index < matchAndValues.values.Length; index++)
                    {
                        if (stringBuilder.Length > 0 && matchVerb.MergeDelimiter != null)
                        {
                            stringBuilder.Append(matchVerb.MergeDelimiter);
                        }

                        var value = matchAndValues.values[index];
                        stringBuilder.AppendFormatted(matchVerb.Format, value, matchAndValues.regex,
                            matchAndValues.match.Groups, matchVerb.BatchValueSeparator, workingDirectory);
                        if (!shouldMerge)
                        {
                            await this.outputter.OutputAsync(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                    }
                }
            }

            if (shouldMerge)
            {
                var mergeFormat = matchVerb.MergeFormat ?? "{0}";
                await this.outputter.OutputAsync(string.Format(mergeFormat, stringBuilder));
                stringBuilder.Clear();
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