// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessRunner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Diagnostics
{
    using System.Diagnostics;

    public class ProcessRunner : IProcessRunner
    {
        public IProcess? Run(ProcessStartInfo processStartInfo)
        {
            var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process == null)
            {
                return null;
            }

            return new Process(process);
        }
    }
}