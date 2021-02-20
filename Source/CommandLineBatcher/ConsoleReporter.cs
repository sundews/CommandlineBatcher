// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandLineBatcher
{
    using System;

    internal class ConsoleReporter : IBatchRunnerReporter
    {
        public void ReportMessage(string line)
        {
            Console.WriteLine(line);
        }
    }
}