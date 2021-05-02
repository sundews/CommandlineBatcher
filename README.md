# CommandlineBatcher

Execute batches of multiple commands in a single command line.

```
Help
 Verbs:
   match/m                           Matches the specified input to patterns and maps it to batches.
     -p    | --patterns              | The patterns (Regex) to be matched in the order they are specified                                | Required
                                       Format: {pattern} => {batch}[,batch]*
                                       Batches may consist of multiple values, separated by the value-separator
                                       Batches can also contain regex group names in the format {group-name}
     Input                                                                                                                               | Required
      -i   | --input                 | The input to be matched                                                                           | Default: [none]
      -isi | --input-stdin           | Indicates that the input should be read from standard input
     -f    | --format                | The format to apply to each batch.                                                                | Default: [none]
     -bs   | --batch-separator       | The character used to split batches.                                                              | Default: |
     -bvs  | --batch-value-separator | The character used to split batch values.                                                         | Default: ,
     -md   | --merge-delimiter       | Specifies the delimiter used between values when merging                                          | Default: [none]
     -m    | --merge-format          | Indicates whether batches should be merged and specifies                                          | Default: [none]
                                       the format to be used for merging
     -lv   | --logging-verbosity     | Logging verbosity: [n]ormal, [e]rrors, [q]uiet, [d]etailed                                        | Default: normal
     <output-path>                   | The output path, if not specified application will output to stdout                               | Default: [none]
 Arguments:                          Executes the specified sequence of commands per batch
  -c       | --commands              | The commands to be executed                                                                       | Required
                                       Format: "[{command}][|{arguments}]"...
                                       Values can be injected by position with {number}
                                       If no command is specified, the argument is sent to standard output
  -bs      | --batch-separation      | Specifies how batches are separated:                                                              | Default: command-line
                                       [c]ommand-line, [n]ew-line, [w]indows-new-line, [u]nix-new-line, [p]ipe, [s]emi-colon, comma
  -bvs     | --batch-value-separator | The batch value separator                                                                         | Default: ,
  Batches with values                                                                                                                    | Required
   -b      | --batches               | The batches to be passed for each command                                                         | Default: [none]
                                       Each batch can contain multiple values separated by the batch value separator
   -bf     | --batches-files         | A list of files containing batches                                                                | Default: [none]
   -bsi    | --batches-stdin         | Indicates that batches should be read from standard input
           | --if                    | A condition for each batch to check if it should run                                              | Default: [none]
                                       Format: [StringComparison:]{lhs} {operator} {rhs}
                                       lhs and rhs can be injected by position with {number}
                                       operators: == equals, |< starts with, >| ends with, >< contains
                                       negations: != not equals, !< not starts with, >! not ends with, <> not contains
                                       StringComparison: O Ordinal, OI OrdinalIgnoreCase, C CurrentCulture,
                                       CI CurrentCultureIgnoreCase, I InvariantCulture, II InvariantCultureIgnoreCase
  -d       | --root-directory        | The directory to search for projects                                                              | Default: Current directory
  -e       | --execution-order       | Specifies whether all commands are executed for the first [b]atch before moving to the next batch | Default: batch
                                       or the first [c]ommand is executed for all batches before moving to the next command
                                       - Finish first [b]atch first
                                       - Finish first [c]ommand first
  -mp      | --max-parallelism       | The degree of parallel execution (1-8)                                                            | Default: 1
                                       Specify "all" for number of cores.
  -p       | --parallelize           | Specifies whether commands or batches run in parallel: [c]ommands, [b]atches                      | Default: commands
  -lv      | --logging-verbosity     | Logging verbosity: [n]ormal, [e]rrors, [q]uiet, [d]etailed                                        | Default: normal
```

## Samples
CommandlineBatcher is being used in various git repositories to automate version tagging of stable releases and wait for all releases to be published to NuGet.

https://github.com/hugener/Sundew.Generator/blob/main/.github/workflows/dotnet.yml

https://github.com/hugener/Sundew.CommandLine/blob/main/.github/workflows/dotnet.yml
