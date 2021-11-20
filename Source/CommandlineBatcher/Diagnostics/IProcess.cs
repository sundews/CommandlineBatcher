// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcess.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Diagnostics;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IProcess
{
    int ExitCode { get; }

    bool HasExited { get; }

    DateTime StartTime { get; }

    DateTime ExitTime { get; }

    int Id { get; }

    string MachineName { get; }

    StreamReader StandardOutput { get; }

    StreamReader StandardError { get; }

    StreamWriter StandardInput { get; }

    ProcessStartInfo StartInfo { get; }

    string ProcessName { get; }

    void WaitForExit();
}