// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Command.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher;

public class Command
{
    public Command(string executable, string arguments)
    {
        this.Executable = executable;
        this.Arguments = arguments;
    }

    public string Executable { get; }

    public string Arguments { get; }
}