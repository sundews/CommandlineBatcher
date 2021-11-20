// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionEvaluator.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CommandlineBatcher.Internal;

using System;
using System.Linq;
using System.Text.RegularExpressions;

public class ConditionEvaluator
{
    private const string Operator = "Operator";
    private const string Lhs = "Lhs";
    private const string Rhs = "Rhs";
    private const string Equal = "==";
    private const string NotEqual = "!=";
    private const string StartsWith = "|<";
    private const string NotStartsWith = "!<";
    private const string EndsWith = ">|";
    private const string NotEndsWith = ">!";
    private const string Contains = "><";
    private const string NotContains = "<>";
    private const string Space = " ";
    private const string Comparison = "Comparison";
    private const string Ordinal = "O";
    private const string OrdinalIgnoreCase = "OI";
    private const string CurrentCulture = "C";
    private const string CurrentCultureIgnoreCase = "CI";
    private const string InvariantCulture = "I";
    private const string InvariantCultureIgnoreCase = "II";

    private static readonly Regex ConditionRegex =
        new(@"^(?:(?<Comparison>OI|CI|II|I|C|O):)?(?<Lhs>[^!=><]+)(?<Operator>==|!=|<<|!<|>>|!>|><|<>)(?<w2>\s)?(?<Rhs>[^!=><]+)$");
    private readonly IConditionEvaluatorReporter conditionEvaluatorReporter;

    public ConditionEvaluator(IConditionEvaluatorReporter conditionEvaluatorReporter)
    {
        this.conditionEvaluatorReporter = conditionEvaluatorReporter;
    }

    /// <summary>
    /// Evaluates the specified condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="values"></param>
    /// <returns>A value indicating whether the condition is true.</returns>
    public bool Evaluate(string? condition, Values values)
    {
        if (string.IsNullOrEmpty(condition))
        {
            return true;
        }

        condition = string.Format(condition, values.Arguments);
        var match = ConditionRegex.Match(condition);
        if (match.Success)
        {
            var comparison = GetComparison(match.Groups[Comparison].Value);
            var lhs = match.Groups[Lhs].Value.AsSpan();
            if (lhs.EndsWith(Space))
            {
                lhs = lhs[..^1];
            }

            var @operator = match.Groups[Operator].Value.Trim();
            var rhs = match.Groups[Rhs].Value;
            var result =  @operator switch
            {
                Equal => lhs.Equals(rhs, comparison),
                NotEqual => !lhs.Equals(rhs, comparison),
                StartsWith => lhs.StartsWith(rhs, comparison),
                NotStartsWith => !lhs.StartsWith(rhs, comparison),
                EndsWith => lhs.EndsWith(rhs, comparison),
                NotEndsWith => !lhs.EndsWith(rhs, comparison),
                Contains => lhs.Contains(rhs, comparison),
                NotContains => !lhs.Contains(rhs, comparison),
                _ => throw new InvalidOperationException($"The operator: {@operator} is not valid."),
            };

            this.conditionEvaluatorReporter.Evaluated(lhs.ToString(), @operator, rhs, result);
            return result;
        }

        return false;
    }

    private StringComparison GetComparison(string comparison)
    {
        if (string.IsNullOrEmpty(comparison))
        {
            return StringComparison.CurrentCulture;
        }

        return comparison switch
        {
            Ordinal => StringComparison.Ordinal,
            OrdinalIgnoreCase => StringComparison.OrdinalIgnoreCase,
            CurrentCulture => StringComparison.CurrentCulture,
            CurrentCultureIgnoreCase => StringComparison.CurrentCultureIgnoreCase,
            InvariantCulture => StringComparison.InvariantCulture,
            InvariantCultureIgnoreCase => StringComparison.InvariantCultureIgnoreCase,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, $"Invalid comparison values: {comparison}")
        };
    }
}