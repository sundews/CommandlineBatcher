// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOutputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using System.Threading.Tasks;

public interface IOutputter
{
    Task OutputAsync(string contents);
}