﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchRunner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandlineBatcher.Diagnostics;
using CommandlineBatcher.Internal;
using Sundew.Base.Collections;
using Sundew.Base.Text;
using Sundew.CommandLine.Extensions;

public class BatchRunner
{
    private const string PipeSeparator = "|";
    private const string SemiColonSeparator = ";";
    private const string CommaSeparator = ",";
    private const string RedirectToFileAppend = ">>";
    private const string RedirectToFile = ">";
    private const string NoPathSpecified = "No path specified";
    private const string UnknownNames = "The following name(s) where not found: ";
    private const string IndicesContainedNullValues = "The following indices contained null values: ";
    private const string EmptyBatchValue = "-";
    private static readonly NamedValues NamedValues = NamedValues.Create(("DQ", "\""), ("NL", Environment.NewLine));
    private readonly IProcessRunner processRunner;
    private readonly IFileSystem fileSystem;
    private readonly ConditionEvaluator conditionEvaluator;
    private readonly IBatchRunnerReporter batchRunnerReporter;

    public BatchRunner(IProcessRunner processRunner, IFileSystem fileSystem, ConditionEvaluator conditionEvaluator, IBatchRunnerReporter batchRunnerReporter)
    {
        this.processRunner = processRunner;
        this.fileSystem = fileSystem;
        this.conditionEvaluator = conditionEvaluator;
        this.batchRunnerReporter = batchRunnerReporter;
    }

    public async Task<IReadOnlyCollection<IProcess>> RunAsync(BatchArguments batchArguments)
    {
        var processes = new ConcurrentQueue<IProcess>();
        var batches = batchArguments.Batches?.ToList() ?? new List<Values>();
        if (batchArguments.BatchesFiles != null)
        {
            foreach (var valuesFile in batchArguments.BatchesFiles)
            {
                if (this.fileSystem.FileExists(valuesFile))
                {
                    var batchesText = GetBatches(this.fileSystem.ReadAllText(valuesFile), batchArguments.BatchSeparation);
                    foreach (var batch in batchesText)
                    {
                        batches.Add(Values.From(batch, batchArguments.BatchValueSeparator));
                    }
                }
                else
                {
                    this.batchRunnerReporter.FileNotFound(valuesFile);
                }
            }
        }

        if (batchArguments.BatchesFromStandardInput)
        {
            var batchesText = GetBatches(await Console.In.ReadToEndAsync(), batchArguments.BatchSeparation);
            foreach (var batch in batchesText)
            {
                batches.Add(Values.From(batch, batchArguments.BatchValueSeparator));
            }
        }

        if (batches.IsEmpty())
        {
            batches.Add(new Values(EmptyBatchValue));
        }

        batches = batches.Where(x => this.conditionEvaluator.Evaluate(batchArguments.Condition, x)).ToList();
        var commandsDegreeOfParallelism = batchArguments.Parallelize == Parallelize.Commands ? batchArguments.MaxDegreeOfParallelism : 1;
        var batchesDegreeOfParallelism = batchArguments.Parallelize == Parallelize.Batches ? batchArguments.MaxDegreeOfParallelism : 1;
        if (batchArguments.ExecutionOrder == ExecutionOrder.Batch)
        {
            Parallel.ForEach(
                batches,
                new ParallelOptions { MaxDegreeOfParallelism = batchesDegreeOfParallelism },
                (values, _) =>
                {
                    Parallel.ForEach(batchArguments.Commands,
                        new ParallelOptions { MaxDegreeOfParallelism = commandsDegreeOfParallelism },
                        (command, _) => RunCommand(batchArguments, command, values, processes));
                });
        }
        else
        {
            Parallel.ForEach(batchArguments.Commands,
                new ParallelOptions { MaxDegreeOfParallelism = commandsDegreeOfParallelism },
                (command, _) =>
                {
                    Parallel.ForEach(batches,
                        new ParallelOptions { MaxDegreeOfParallelism = batchesDegreeOfParallelism },
                        (values, _) => RunCommand(batchArguments, command, values, processes));
                });
        }

        return processes;
    }

    private IEnumerable<string> GetBatches(string batchesText, BatchSeparation batchSeparation)
    {
        return batchSeparation switch
        {
            BatchSeparation.CommandLine => batchesText.AsMemory().Trim().ParseCommandLineArguments().Select(x => x.ToString()),
            BatchSeparation.NewLine => batchesText.Split(Strings.NewLine, StringSplitOptions.RemoveEmptyEntries),
            BatchSeparation.WindowsNewLine => batchesText.Split(Strings.WindowsNewLine, StringSplitOptions.RemoveEmptyEntries),
            BatchSeparation.UnixNewLine => batchesText.Split(Strings.UnixNewLine, StringSplitOptions.RemoveEmptyEntries),
            BatchSeparation.Pipe => batchesText.Split(PipeSeparator, StringSplitOptions.RemoveEmptyEntries),
            BatchSeparation.SemiColon => batchesText.Split(SemiColonSeparator, StringSplitOptions.RemoveEmptyEntries),
            BatchSeparation.Comma => batchesText.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries),
            _ => throw new ArgumentOutOfRangeException(nameof(batchSeparation), batchSeparation, $"Invalid batch separation value: {batchSeparation}")
        };
    }

    private void RunCommand(BatchArguments batchArguments, Command command, Values values, ConcurrentQueue<IProcess> processes)
    {
        if (string.IsNullOrEmpty(command.Executable))
        {
            Console.WriteLine(command.Arguments, values.Arguments);
            return;
        }

        if (command.Executable.StartsWith(RedirectToFileAppend))
        {
            var path = GetFilePath(command.Executable.AsSpan(2), batchArguments);
            var (content, isValid) = Format(command.Arguments, values.Arguments);
            if (isValid)
            {
                this.fileSystem.AppendAllText(path, content, EncodingHelper.GetEncoding(batchArguments.FileEncoding));
            }
            else
            {
                throw new InvalidOperationException(content);
            }

            return;
        }

        if (command.Executable.StartsWith(RedirectToFile))
        {
            var path = GetFilePath(command.Executable.AsSpan(1), batchArguments);
            this.fileSystem.WriteAllText(path, string.Format(command.Arguments, values.Arguments), EncodingHelper.GetEncoding(batchArguments.FileEncoding));
            return;
        }

        try
        {
            var processStartInfo =
                new ProcessStartInfo(command.Executable, string.Format(command.Arguments, values.Arguments))
                {
                    WorkingDirectory = batchArguments.RootDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };
            var process = this.processRunner.Run(processStartInfo);
            if (process != null)
            {
                this.batchRunnerReporter.Started(process);
                processes.Enqueue(process);
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        this.batchRunnerReporter.ReportMessage(process, line);
                    }
                }

                process.WaitForExit();
                this.batchRunnerReporter.ProcessExited(process);
            }
        }
        catch (Win32Exception e)
        {
            if (e.NativeErrorCode == 2)
            {
                throw new InvalidOperationException($"Could not find the executable: {command.Executable}", e);
            }

            throw;
        }
    }

    private string GetFilePath(ReadOnlySpan<char> command, BatchArguments batchArguments)
    {
        var path = command.Trim().ToString();
        if (string.IsNullOrEmpty(path))
        {
            path = batchArguments.OutputFilePath;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException(NoPathSpecified);
            }
        }

        return path;
    }
    
    internal static (string Log, bool IsValid) Format(
        string logFormat,
        string?[] arguments)
    {
        const string separator = ", ";
        var result = NamedFormatString.Format(CultureInfo.CurrentCulture, logFormat, NamedValues, arguments);
        return result switch
        {
            StringFormatted stringFormatted => (stringFormatted.Result, true),
            FormatContainedUnknownNames formatContainedUnknownNames => (new StringBuilder(UnknownNames).AppendItems(formatContainedUnknownNames.Names, (builder, name) => builder.Append(name), separator).ToString(), false),
            ArgumentsContainedNullValues argumentsContainedNullValues => (new StringBuilder(IndicesContainedNullValues).AppendItems(argumentsContainedNullValues.NullArguments, (builder, namedIndex) => builder.Append($"{namedIndex.Name}({namedIndex.Index})"), separator).ToString(), false),
        };
    }
}