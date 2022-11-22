﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystem.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Internal;

using System.IO;
using System.Text;

internal class FileSystem : IFileSystem
{
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    public void AppendAllText(string path, string content, Encoding encoding)
    {
        File.AppendAllText(path, content, encoding);
    }

    public void WriteAllText(string path, string content, Encoding encoding)
    {
        File.WriteAllText(path, content, encoding);
    }
}