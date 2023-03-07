// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileOutputter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Match;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class FileOutputter : IOutputter
{
    private readonly string outputPath;
    private readonly bool appendToFile;
    private readonly Encoding encoding;

    public FileOutputter(string outputPath, bool appendToFile, Encoding encoding)
    {
        this.outputPath = outputPath;
        this.appendToFile = appendToFile;
        this.encoding = encoding;
    }

    public Task OutputAsync(string contents)
    {
        var directoryPath = Path.GetDirectoryName(Path.GetFullPath(this.outputPath));

        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (this.appendToFile)
        {
            return File.AppendAllTextAsync(this.outputPath, contents + Environment.NewLine, this.encoding);
        }

        return File.WriteAllTextAsync(this.outputPath, contents + Environment.NewLine, this.encoding);
    }
}