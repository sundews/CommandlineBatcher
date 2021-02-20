namespace CommandLineBatcher
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLineBatcher.Diagnostics;
    using Sundew.Base.Computation;
    using Sundew.CommandLine;

    class Program
    {
        static async Task Main()
        {
            Console.WriteLine(Environment.CommandLine);
            var commandLineParser = new CommandLineParser<int, int>();
            commandLineParser.WithArguments(new BatchArguments(), Handle);

            var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
            if (!result)
            {
                result.WriteToConsole();
            }
        }

        private static async ValueTask<Result<int, ParserError<int>>> Handle(BatchArguments arguments)
        {
            var batchRunner = new BatchRunner(new ProcessRunner(), new ConsoleReporter());
            var processes = await batchRunner.RunAsync(arguments);
            var failedProcesses = processes.Where(x => x.ExitCode != 0).ToList();
            foreach (var process in failedProcesses) 
            {
                Console.WriteLine($@"{process.StartInfo.FileName} {process.StartInfo.Arguments} failed with {process.ExitCode}");
            }

            return Result.From(failedProcesses.Count == 0, 0, new ParserError<int>(-1));
        }
    }
}
