// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregateOutputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AggregateOutputter : IOutputter
    {
        private readonly IReadOnlyList<IOutputter> outputters;

        public AggregateOutputter(IReadOnlyList<IOutputter> outputters)
        {
            this.outputters = outputters;
        }

        public async Task OutputAsync(string contents)
        {
            for (int i = 0; i < this.outputters.Count; i++)
            {
                await this.outputters[i].OutputAsync(contents);
            }
        }
    }
}
