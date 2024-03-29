﻿namespace CommandlineBatcher;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandlineBatcher.Diagnostics;
using CommandlineBatcher.Internal;
using CommandlineBatcher.Match;
using Sundew.Base;
using Sundew.CommandLine;

class Program
{
    static async Task<int> Main()
    {
        Console.WriteLine(Environment.CommandLine);
        var commandLineParser = new CommandLineParser<int, int>();
        commandLineParser.AddVerb(new MatchVerb(), ExecuteMatchAsync);
        commandLineParser.WithArguments(new BatchArguments(), Handle);

        var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
        if (!result.IsSuccess)
        {
            result.WriteToConsole();
            return result.Error.Info;
        }

        return result.Value;
    }

    private static async ValueTask<R<int, ParserError<int>>> Handle(BatchArguments arguments)
    {
        var consoleReporter = new ConsoleReporter(arguments.Verbosity);
        var batchRunner = new BatchRunner(new ProcessRunner(), new FileSystem(), new ConditionEvaluator(consoleReporter), consoleReporter);
        var processes = await batchRunner.RunAsync(arguments);
        var failedProcesses = processes.Where(x => x.ExitCode != 0).ToList();
        foreach (var process in failedProcesses)
        {
            consoleReporter.Error(process);
        }

        return R.From(failedProcesses.Count == 0, 0, new ParserError<int>(-1));
    }

    private static async ValueTask<R<int, ParserError<int>>> ExecuteMatchAsync(MatchVerb matchVerb)
    {
        var outputters = new List<IOutputter>();
        if (!matchVerb.SkipConsoleOutput)
        {
            outputters.Add(new ConsoleOutputter());
        }

        if (matchVerb.OutputFile != null)
        {
            outputters.Add(new FileOutputter(matchVerb.OutputFile, matchVerb.Overwrite, EncodingHelper.GetEncoding(matchVerb.FileEncoding)));
        }

        IInputter inputter = matchVerb.UseStandardInput ? new ConsoleInputter() : new Inputter(matchVerb.Inputs);
        var matchFacade = new MatchFacade(inputter, new AggregateOutputter(outputters), new FileSystem(), new ConsoleReporter(matchVerb.Verbosity));
        return R.Success(await matchFacade.MatchAsync(matchVerb));
    }
}