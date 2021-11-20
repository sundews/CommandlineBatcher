﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchSeparation.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher;

public enum BatchSeparation
{
    CommandLine,
    NewLine,
    WindowsNewLine,
    UnixNewLine,
    Pipe,
    SemiColon,
    Comma
}