// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBatchRunnerReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher
{
    using CommandlineBatcher.Diagnostics;

    public interface IBatchRunnerReporter
    {
        void Started(IProcess process);

        void ReportMessage(IProcess process, string line);

        void ProcessExited(IProcess process);
    }
}