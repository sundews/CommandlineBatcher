// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBatchRunnerReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandLineBatcher
{
    public interface IBatchRunnerReporter
    {
        void ReportMessage(string line);
    }
}