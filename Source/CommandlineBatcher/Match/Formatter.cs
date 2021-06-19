// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Formatter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Text.RegularExpressions;
    using Sundew.Base.Collections;
    using Sundew.Base.Memory;
    using Sundew.Base.Text;
    using SplitAction = Sundew.Base.Memory.SplitAction;

    public static class Formatter
    {
        private const string WindowsNewLine = "{WNL}";
        private const string UnixNewLine = "{UNL}";
        private const string NewLine = "{NL}";
        private const string CurrentDirectory = "{CurrentDirectory}";
        private const string DoubleQuoteName = "{DQ}";
        private const string DoubleQuote = "\"";

        public static void AppendFormat(StringBuilder stringBuilder, string? format, string value, Regex regex, GroupCollection matchGroups, char batchValueSeparator, string workingDirectory)
        {
            format = ReplaceKnownCharacters(format, workingDirectory);
            value = ReplaceKnownCharacters(value, workingDirectory)!;
            foreach (var groupName in regex.GetGroupNames())
            {
                if (groupName == regex.GroupNumberFromName(groupName).ToString())
                {
                    continue;
                }

                var group = matchGroups[groupName];
                if (group.Success)
                {
                    value = value.Replace($@"{{{groupName}}}", group.Value);
                }
            }

            if (format == null)
            {
                stringBuilder.Append(value);
                return;
            }

            var lastWasInSeparator = false;
            var values = value.AsMemory().Split((character, index, _) =>
                {
                    var wasInSeparator = lastWasInSeparator;
                    if (wasInSeparator)
                    {
                        lastWasInSeparator = false;
                    }

                    if (character == batchValueSeparator)
                    {
                        if (wasInSeparator)
                        {
                            return SplitAction.Include;
                        }

                        if (index == 0)
                        {
                            return SplitAction.Split;
                        }

                        lastWasInSeparator = true;
                        return SplitAction.Ignore;
                    }

                    if (wasInSeparator)
                    {
                        return SplitAction.SplitAndInclude;
                    }

                    return SplitAction.Include;
                },
                SplitOptions.None).ToArray(x => x.ToString());

            stringBuilder.AppendFormat(format, values);
        }

        private static string? ReplaceKnownCharacters(string? value, string currentDirectory)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Replace(WindowsNewLine, Strings.WindowsNewLine)
                .Replace(UnixNewLine, Strings.UnixNewLine)
                .Replace(NewLine, Environment.NewLine)
                .Replace(DoubleQuoteName, DoubleQuote)
                .Replace(CurrentDirectory, currentDirectory);
        }
    }
}