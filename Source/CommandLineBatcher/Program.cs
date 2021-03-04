namespace CommandlineBatcher
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandlineBatcher.Diagnostics;
    using CommandlineBatcher.Internal;
    using Sundew.Base.Computation;
    using Sundew.CommandLine;

    class Program
    {
        static async Task<int> Main()
        {
            Console.WriteLine(Environment.CommandLine);
            var commandLineParser = new CommandLineParser<int, int>();
            commandLineParser.WithArguments(new BatchArguments(), Handle);

            var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
            if (!result)
            {
                result.WriteToConsole();
                return result.Error.Info;
            }

            return result.Value;
        }

        private static async ValueTask<Result<int, ParserError<int>>> Handle(BatchArguments arguments)
        {
            var consoleReporter = new ConsoleReporter(arguments.Verbosity);
            var batchRunner = new BatchRunner(new ProcessRunner(), new FileSystem(), consoleReporter);
            var processes = await batchRunner.RunAsync(arguments);
            var failedProcesses = processes.Where(x => x.ExitCode != 0).ToList();
            foreach (var process in failedProcesses)
            {
                consoleReporter.Error(process);
            }

            return Result.From(failedProcesses.Count == 0, 0, new ParserError<int>(-1));
        }
    }
}
