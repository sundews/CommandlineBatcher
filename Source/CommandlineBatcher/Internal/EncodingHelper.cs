// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodingHelper.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Internal;

using System.Text;
using Sundew.Base.Text;

internal class EncodingHelper
{
    public static Encoding GetEncoding(string? encodingName)
    {
        var encoding = encodingName?.ToLower() switch
        {
            "utf8" => Encoding.UTF8,
            "utf16" => Encoding.Unicode,
            "unicode" => Encoding.Unicode,
            Strings.Empty => Encoding.Default,
            null => Encoding.Default,
            _ => Encoding.GetEncoding(encodingName),
        };

        return encoding;
    }
}