// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchRunner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandlineBatcher.Diagnostics;
    using CommandlineBatcher.Internal;
    using Sundew.CommandLine.Extensions;

    public class BatchRunner
    {
        private readonly IProcessRunner processRunner;
        private readonly IFileSystem fileSystem;
        private readonly IBatchRunnerReporter batchRunnerReporter;

        public BatchRunner(IProcessRunner processRunner, IFileSystem fileSystem, IBatchRunnerReporter batchRunnerReporter)
        {
            this.processRunner = processRunner;
            this.fileSystem = fileSystem;
            this.batchRunnerReporter = batchRunnerReporter;
        }

        public Task<IReadOnlyCollection<IProcess>> RunAsync(BatchArguments batchArguments)
        {
            var processes = new ConcurrentQueue<IProcess>();
            var batches = batchArguments.Batches?.ToList() ?? new List<Values>();
            if (batchArguments.BatchesFiles != null)
            {
                foreach (var valuesFile in batchArguments.BatchesFiles)
                {
                    var batchesText = this.fileSystem.ReadAllText(valuesFile).Trim().AsMemory().ParseCommandLineArguments();
                    foreach (var batch in batchesText)
                    {
                        batches.Add(Values.From(batch, batchArguments.BatchValueSeparator));
                    }
                }
            }

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

            return Task.FromResult<IReadOnlyCollection<IProcess>>(processes);
        }

        private void RunCommand(BatchArguments batchArguments, Command command, Values values, ConcurrentQueue<IProcess> processes)
        {
            var processStartInfo = new ProcessStartInfo(command.Executable, string.Format(command.Arguments, values.Arguments))
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
    }
}