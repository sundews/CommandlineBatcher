# CommandlineBatcher

Execute batches of multiple commands in a single command line.

```
Help
 Arguments:
  -c   | --commands              | The commands to be executed                                                                       | Required
                                   Format: "{command}[|{arguments}]"...
                                   Values can be injected by position with {number}
  -s   | --batch-value-separator | The batch value separator                                                                         | Default: |
  Batches with values                                                                                                                | Required
   -b  | --batches               | The batches to be passed for each command                                                         | Default: [none]
                                   Each batch can contain multiple values separated by the batch value separator
   -bf | --batches-files         | A list of files containing batches                                                                | Default: [none]
  -d   | --root-directory        | The directory to search for projects                                                              | Default: Current directory
  -e   | --execution-order       | Specifies whether all commands are executed for the first [b]atch before moving to the next batch | Default: batch
                                   or the first [c]ommand is executed for all batches before moving to the next command
                                   - Finish first [b]atch first
                                   - Finish first [c]ommand first
  -mp  | --max-parallelism       | The degree of parallel execution (1-8)                                                            | Default: 1
                                   Specify "all" for number of cores.
  -p   | --parallelize           | Specifies whether commands or batches run in parallel: [c]ommands, [b]atches                      | Default: commands
  -lv  | --logging-verbosity     | Logging verbosity: [n]ormal, [e]rrors, [q]uiet, [d]etailed                                        | Default: normal
```

## Samples
CommandlineBatcher is being used in various git repositories to automate version tagging of stable releases and wait for all releases to be published to NuGet.

https://github.com/hugener/Sundew.Generator/blob/main/azure-pipelines.yml

https://github.com/hugener/Sundew.CommandLine/blob/master/azure-pipelines.yml
