// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchArguments.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Sundew.Base.Primitives.Numeric;
using Sundew.Base.Text;
using Sundew.CommandLine;

public class BatchArguments : IArguments
{
    private const string All = "All";
    private readonly List<Command> commands;
    private readonly List<Values>? batches;
    private readonly List<string>? batchesFiles;

    public BatchArguments(
        List<Command> commands,
        BatchSeparation batchSeparation = BatchSeparation.CommandLine,
        string? batchValueSeparator = null,
        List<Values>? batches = null,
        List<string>? batchesFiles = null,
        bool batchesFromStandardInput = false,
        string? condition = null,
        string? rootDirectory = null,
        ExecutionOrder executionOrder = ExecutionOrder.Batch,
        int maxDegreeOfParallelism = 1,
        Parallelize parallelize = Parallelize.Commands,
        Verbosity verbosity = default,
        string? fileEncoding = default,
        string? outputFilePath = default)
    {
        this.commands = commands;
        this.batches = batches;
        this.batchesFiles = batchesFiles;
        this.BatchSeparation = batchSeparation;
        this.BatchesFromStandardInput = batchesFromStandardInput;
        this.BatchValueSeparator = batchValueSeparator ?? ",";
        this.Condition = condition;
        this.RootDirectory = rootDirectory ?? Directory.GetCurrentDirectory();
        this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        this.Parallelize = parallelize;
        this.ExecutionOrder = executionOrder;
        this.Verbosity = verbosity;
        this.FileEncoding = fileEncoding ?? Encoding.Default.BodyName;
        this.FileEncoding = outputFilePath;
    }

    public BatchArguments()
        : this(new List<Command>(), BatchSeparation.CommandLine, null, new List<Values>(), new List<string>())
    {
    }

    public IReadOnlyList<Command> Commands => this.commands;

    public string RootDirectory { get; private set; }

    public IReadOnlyList<string>? BatchesFiles => this.batchesFiles;

    public IReadOnlyList<Values>? Batches => this.batches;

    public bool BatchesFromStandardInput { get; private set; }

    public BatchSeparation BatchSeparation { get; private set; }

    public string BatchValueSeparator { get; private set; }

    public string? Condition { get; private set; }

    public int MaxDegreeOfParallelism { get; private set; }

    public Parallelize Parallelize { get; private set; }

    public ExecutionOrder ExecutionOrder { get; private set; }

    public Verbosity Verbosity { get; private set; }

    public string? FileEncoding { get; private set; }

    public string? OutputFilePath { get; private set; }
    
    public string HelpText { get; } = "Executes the specified sequence of commands per batch";
        
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddRequiredList("c", "commands", this.commands, this.SerializeCommand, this.DeserializeCommand, @$"The commands to be executed{Environment.NewLine}Format: ""[{{command}}][|{{arguments}}]""...{Environment.NewLine}Values can be injected by position with {{number}}{Environment.NewLine}If no command is specified, the argument is sent to standard output{Environment.NewLine}Use command "">> {{file path}}"" to append to file{Environment.NewLine}Use command ""> {{file path}}"" to write to file", true);
        argumentsBuilder.AddOptionalEnum("bs", "batch-separation", () => this.BatchSeparation, s => this.BatchSeparation = s, @"Specifies how batches are separated:
{0}");
        argumentsBuilder.AddOptional("bvs", "batch-value-separator", () => this.BatchValueSeparator, s => this.BatchValueSeparator = s, "The batch value separator");
        argumentsBuilder.RequireAnyOf("Batches with values", x => x
            .AddList("b", "batches", this.batches!, this.SerializeBatch, this.DeserializeBatch, @$"The batches to be passed for each command
Each batch can contain multiple values separated by the batch value separator", true)
            .AddList("bf", "batches-files", this.batchesFiles!, "A list of files containing batches", true)
            .AddSwitch("bsi", "batches-stdin", this.BatchesFromStandardInput, b => this.BatchesFromStandardInput = b, "Indicates that batches should be read from standard input"));
        argumentsBuilder.AddOptional(null, "if", () => this.Condition, c => this.Condition = c, @$"A condition for each batch to check if it should run
Format: [StringComparison:]{{lhs}} {{operator}} {{rhs}}
lhs and rhs can be injected by position with {{number}}
operators: == equals, |< starts with, >| ends with, >< contains
negations: != not equals, !< not starts with, >! not ends with, <> not contains
StringComparison: O Ordinal, OI OrdinalIgnoreCase, C CurrentCulture,
CI CurrentCultureIgnoreCase, I InvariantCulture, II InvariantCultureIgnoreCase", true);
        argumentsBuilder.AddOptional("d", "root-directory", () => this.RootDirectory, s => this.RootDirectory = s, "The directory to search for projects", true, defaultValueText: "Current directory");
        argumentsBuilder.AddOptionalEnum("e", "execution-order", () => this.ExecutionOrder, v => this.ExecutionOrder = v, @$"Specifies whether all commands are executed for the first {{1}} before moving to the next batch
or the first {{2}} is executed for all batches before moving to the next command
- Finish first {{1}} first
- Finish first {{2}} first");
        argumentsBuilder.AddOptional("mp", "max-parallelism", () => this.MaxDegreeOfParallelism.ToString(), this.DeserializeMaxParallelism, @$"The degree of parallel execution (1-{Environment.ProcessorCount}){Environment.NewLine}Specify ""all"" for number of cores.");
        argumentsBuilder.AddOptionalEnum("p", "parallelize", () => this.Parallelize, v => this.Parallelize = v, "Specifies whether commands or batches run in parallel: {0}");
        argumentsBuilder.AddOptionalEnum("lv", "logging-verbosity", () => this.Verbosity, v => this.Verbosity = v, "Logging verbosity: {0}");
        argumentsBuilder.AddOptional("fe", "file-encoding", () => this.FileEncoding, s => this.FileEncoding = s, @$"The name of the encoding e.g. utf-8, utf-16/unicode.");
        argumentsBuilder.AddOptional("o", "output-file", () => this.OutputFilePath, s => this.OutputFilePath = s, @$"The file path output redirect commands that do not specify a file path to.");
    }

    private Command DeserializeCommand(string commandWithArguments, CultureInfo arg2)
    {
        var args = commandWithArguments.Split('|');
        return args.Length switch
        {
            1 => new Command(args[0], string.Empty),
            2 => new Command(args[0], args[1]),
            _ => throw new ArgumentException(@$"Argument {commandWithArguments} did not follow the format ""{{command}}[|{{arguments}}]""...")
        };
    }

    private string SerializeCommand(Command command, CultureInfo arg2)
    {
        return $"{command.Executable}|{command.Arguments}";
    }

    private Values DeserializeBatch(string values, CultureInfo arg2)
    {
        return Values.From(values, this.BatchValueSeparator!);
    }

    private string SerializeBatch(Values values, CultureInfo arg2)
    {
        return values.Arguments.JoinToString(this.BatchValueSeparator);
    }

    private void DeserializeMaxParallelism(string s)
    {
        if (All.Equals(s, StringComparison.InvariantCultureIgnoreCase))
        {
            this.MaxDegreeOfParallelism = Environment.ProcessorCount;
            return;
        }
            
        this.MaxDegreeOfParallelism = Interval.From(1, Environment.ProcessorCount).Limit(int.Parse(s));
    }
}