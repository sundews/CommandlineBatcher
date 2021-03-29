// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionEvaluator.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Internal
{
    public class ConditionEvaluator
    {
        private readonly IConditionEvaluatorReporter conditionEvaluatorReporter;

        public ConditionEvaluator(IConditionEvaluatorReporter conditionEvaluatorReporter)
        {
            this.conditionEvaluatorReporter = conditionEvaluatorReporter;
        }

        /// <summary>
        /// Evaluates the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A value indicating whether the condition is true.</returns>
        public bool Evaluate(string? condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return true;
            }

            return false;
        }
    }
}