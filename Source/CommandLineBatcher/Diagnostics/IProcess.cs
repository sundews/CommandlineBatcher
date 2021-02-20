// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcess.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandLineBatcher.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProcess
    {
        public int ExitCode { get; }

        public bool HasExited { get; }

        public DateTime StartTime { get; }

        public DateTime ExitTime { get; }

        public int Id { get; }

        public string MachineName { get; }

        public StreamReader StandardOutput { get; }

        public StreamReader StandardError { get; }

        public StreamWriter StandardInput { get; }
        ProcessStartInfo StartInfo { get; }
        string ProcessName { get; }

        public Task WaitForExistAsync(CancellationToken cancellationToken);
    }
}