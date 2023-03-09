// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleInputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ConsoleInputter : IInputter
{
    public async Task<IReadOnlyList<string>> GetInputAsync()
    {
        return new[] { await Console.In.ReadToEndAsync() };
    }
}