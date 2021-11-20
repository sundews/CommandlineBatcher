﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher;

using System;
using CommandlineBatcher.Diagnostics;
using CommandlineBatcher.Internal;
using CommandlineBatcher.Match;

internal class ConsoleReporter : IBatchRunnerReporter, IConditionEvaluatorReporter, IMatchReporter
{
    private readonly Verbosity verbosity;

    public ConsoleReporter(Verbosity verbosity)
    {
        this.verbosity = verbosity;
    }

    public void Started(IProcess process)
    {
        if (this.verbosity == Verbosity.Detailed || this.verbosity == Verbosity.Normal)
        {
            Console.WriteLine($"Started: {process.StartInfo.FileName} {process.StartInfo.Arguments} ({process.Id})");
        }
    }

    public void ReportMessage(IProcess process, string line)
    {
        if (this.verbosity == Verbosity.Detailed)
        {
            Console.WriteLine($"{process.StartInfo.FileName} ({process.Id}) reported:{Environment.NewLine}{line}");
            return;
        }

        Console.WriteLine(line);
    }

    public void ProcessExited(IProcess process)
    {
        if (this.verbosity == Verbosity.Detailed)
        {
            Console.WriteLine($"{process.StartInfo.FileName} ({process.Id}) exited with exit code: {process.ExitCode}");
        }
    }

    public void FileNotFound(string valuesFile)
    {
        if (this.verbosity != Verbosity.Quiet)
        {
            Console.WriteLine($@"{valuesFile} was not found and will be ignored");
        }
    }

    public void Error(IProcess process)
    {
        if (this.verbosity != Verbosity.Quiet)
        {
            Console.WriteLine($@"{process.StartInfo.FileName} {process.StartInfo.Arguments} ({process.Id}) failed with {process.ExitCode}");
        }
    }

    public void Evaluated(string lhs, string @operator, string rhs, bool result)
    {
        if (this.verbosity != Verbosity.Quiet)
        {
            Console.WriteLine($@"Evaluated '{lhs}' {@operator} '{rhs}' to {result}");
        }
    }

    public void Exception(Exception exception)
    {
        var backgroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.ToString());
        Console.ForegroundColor = backgroundColor;
    }

    public void Report(string message)
    {
        if (this.verbosity != Verbosity.Quiet)
        {
            Console.WriteLine(message);
        }
    }
}