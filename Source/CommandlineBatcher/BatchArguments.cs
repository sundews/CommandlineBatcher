// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchArguments.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Sundew.Base.Collections;
    using Sundew.Base.Numeric;
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
            string? batchSeparator = null,
            List<Values>? batches = null,
            List<string>? batchesFiles = null,
            string? rootDirectory = null,
            ExecutionOrder executionOrder = ExecutionOrder.Batch,
            int maxDegreeOfParallelism = 1,
            Parallelize parallelize = Parallelize.Commands,
            Verbosity verbosity = default)
        {
            this.commands = commands;
            this.batches = batches;
            this.batchesFiles = batchesFiles;
            this.BatchValueSeparator = batchSeparator ?? "|";
            this.RootDirectory = rootDirectory ?? Directory.GetCurrentDirectory();
            this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            this.Parallelize = parallelize;
            this.ExecutionOrder = executionOrder;
            this.Verbosity = verbosity;
        }

        public BatchArguments()
            : this(new List<Command>(), null, new List<Values>(), new List<string>())
        {
        }

        public IReadOnlyList<Command> Commands => this.commands;

        public string RootDirectory { get; private set; }

        public IReadOnlyList<string>? BatchesFiles => this.batchesFiles;

        public IReadOnlyList<Values>? Batches => this.batches;

        public string BatchValueSeparator { get; private set; }

        public int MaxDegreeOfParallelism { get; private set; }

        public Parallelize Parallelize { get; private set; }

        public ExecutionOrder ExecutionOrder { get; private set; }

        public Verbosity Verbosity { get; private set; }

        public void Configure(IArgumentsBuilder argumentsBuilder)
        {
            argumentsBuilder.OptionsHelpOrder = OptionsHelpOrder.AsAdded;
            argumentsBuilder.AddRequiredList("c", "commands", this.commands, this.SerializeCommand, this.DeserializeCommand, @$"The commands to be executed{Environment.NewLine}Format: ""{{command}}[|{{arguments}}]""...{Environment.NewLine}Values can be injected by position with {{number}}", true);
            argumentsBuilder.AddOptional("s", "batch-value-separator", () => this.BatchValueSeparator, s => this.BatchValueSeparator = s, "The batch value separator");
            argumentsBuilder.RequireAnyOf("Batches with values", x => x
                .AddList("b", "batches", this.batches!, this.SerializeBatch, this.DeserializeBatch, $"The batches to be passed for each command{Environment.NewLine}Each batch can contain multiple values separated by the batch value separator", true)
                .AddList("bf", "batches-files", this.batchesFiles!, "A list of files containing batches", true));
            argumentsBuilder.AddOptional("d", "root-directory", () => this.RootDirectory, s => this.RootDirectory = s, "The directory to search for projects", true, defaultValueText: "Current directory");
            argumentsBuilder.AddOptionalEnum("e", "execution-order", () => this.ExecutionOrder, v  => this.ExecutionOrder = v, $"Specifies whether all commands are executed for the first {{1}} before moving to the next batch{Environment.NewLine}or the first {{2}} is executed for all batches before moving to the next command{Environment.NewLine}- Finish first {{1}} first{Environment.NewLine}- Finish first {{2}} first");
            argumentsBuilder.AddOptional("mp", "max-parallelism", () => this.MaxDegreeOfParallelism.ToString(), this.DeserializeMaxParallelism, @$"The degree of parallel execution (1-{Environment.ProcessorCount}){Environment.NewLine}Specify ""all"" for number of cores.");
            argumentsBuilder.AddOptionalEnum("p", "parallelize", () => this.Parallelize, v  => this.Parallelize = v, "Specifies whether commands or batches run in parallel: {0}");
            argumentsBuilder.AddOptionalEnum("lv", "logging-verbosity", () => this.Verbosity, v  => this.Verbosity = v, "Logging verbosity: {0}");
        }

        private Command DeserializeCommand(string commandWithArguments, CultureInfo arg2)
        {
            var args = commandWithArguments.Split('|', StringSplitOptions.RemoveEmptyEntries);
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
            return values.Arguments.AggregateToStringBuilder(
                (builder, s) => builder.Append(s).Append(this.BatchValueSeparator),
                builder => builder.ToStringFromEnd(1));
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
}