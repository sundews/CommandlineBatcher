// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Inputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using System.Threading.Tasks;

public class Inputter : IInputter
{
    private readonly string input;

    public Inputter(string input)
    {
        this.input = input;
    }

    public Task<string> GetInputAsync()
    {
        return Task.FromResult(input);
    }
}