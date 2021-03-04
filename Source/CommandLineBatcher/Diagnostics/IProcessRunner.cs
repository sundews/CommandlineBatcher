// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessRunner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Diagnostics
{
    using System.Diagnostics;

    public interface IProcessRunner
    {
        IProcess? Run(ProcessStartInfo processStartInfo);
    }
}