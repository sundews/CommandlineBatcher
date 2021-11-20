﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConditionEvaluatorReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Internal;

public interface IConditionEvaluatorReporter
{
    void Evaluated(string lhs, string @operator, string rhs, bool result);
}