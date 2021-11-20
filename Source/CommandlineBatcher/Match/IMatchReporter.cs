// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMatchReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using System;

public interface IMatchReporter
{
    void Exception(Exception exception);

    void Report(string message);
}