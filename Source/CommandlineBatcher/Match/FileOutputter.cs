// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileOutputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class FileOutputter : IOutputter
    {
        private readonly string outputPath;

        public FileOutputter(string outputPath)
        {
            this.outputPath = outputPath;
        }

        public Task OutputAsync(string contents)
        {
            var directoryPath = Path.GetDirectoryName(Path.GetFullPath(this.outputPath));

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return File.WriteAllTextAsync(this.outputPath, contents + Environment.NewLine);
        }
    }
}