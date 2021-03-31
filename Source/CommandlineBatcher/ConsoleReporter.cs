// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher
{
    using System;
    using CommandlineBatcher.Diagnostics;
    using CommandlineBatcher.Internal;

    internal class ConsoleReporter : IBatchRunnerReporter, IConditionEvaluatorReporter
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
    }
}