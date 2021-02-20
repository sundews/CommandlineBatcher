// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchRunner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandLineBatcher
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLineBatcher.Diagnostics;
    using Sundew.Base.Collections;
    using Process = CommandLineBatcher.Diagnostics.Process;

    public class BatchRunner
    {
        private readonly IProcessRunner processRunner;
        private readonly IBatchRunnerReporter batchRunnerReporter;

        public BatchRunner(IProcessRunner processRunner, IBatchRunnerReporter batchRunnerReporter)
        {
            this.processRunner = processRunner;
            this.batchRunnerReporter = batchRunnerReporter;
        }

        public async Task<IReadOnlyList<IProcess>> RunAsync(BatchArguments batchArguments)
        {
            var processes = new List<IProcess>();
            foreach (var values in batchArguments.Values)
            {
                foreach (var command in batchArguments.Commands)
                {
                    var process = this.processRunner.Run(new ProcessStartInfo(command.Executable, string.Format(command.Arguments, values.Arguments))
                    {
                        WorkingDirectory = batchArguments.RootDirectory,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    });
                    if (process != null)
                    {
                        processes.Add(process);
                        while (!process.StandardOutput.EndOfStream)
                        {
                            var line = await process.StandardOutput.ReadLineAsync();
                            if (line != null)
                            {
                                this.batchRunnerReporter.ReportMessage(line);
                            }
                        }

                        await process.WaitForExistAsync(CancellationToken.None);
                    }
                }
            }

            return processes;
        }
    }
}