// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleOutputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match
{
    using System;
    using System.Threading.Tasks;

    public class ConsoleOutputter : IOutputter
    {
        public Task OutputAsync(string contents)
        {
            Console.WriteLine(contents);
            return Task.CompletedTask;
        }
    }
}