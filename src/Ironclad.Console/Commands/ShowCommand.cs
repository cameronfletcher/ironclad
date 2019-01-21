// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

// TODO (Cameron): Remove suppression (below) when StyleCop supports C# 7.0 tuples.
#pragma warning disable SA1008

namespace Ironclad.Console.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Ironclad.Client;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using collections = System.Collections.Generic;

    internal static class ShowCommand
    {
        public static void Configure(CommandLineApplication app, CommandLineOptions commandLineOptions, ShowCommandOptions showCommandOptions)
        {
            // description
            app.Description = $"Displays the {showCommandOptions.Type}s";
            app.HelpOption();

            // arguments
            var argument = app.Argument(showCommandOptions.ArgumentName, showCommandOptions.ArgumentDescription, false);

            // options
            var optionSkip = app.Option("-s|--skip", $"The number of {showCommandOptions.Type}s to skip", CommandOptionType.SingleValue);
            var optionTake = app.Option("-t|--take", $"The number of {showCommandOptions.Type}s to take", CommandOptionType.SingleValue);

            // action (for this command)
            app.OnExecute(
                () =>
                {
                    if (!string.IsNullOrEmpty(argument.Value) && !argument.Value.EndsWith('*'))
                    {
                        commandLineOptions.Command = showCommandOptions.DisplayCommand(argument.Value);

                        return;
                    }

                    var startsWith = argument.Value?.TrimEnd('*') ?? argument.Value;

                    var skip = 0;
                    if (optionSkip.HasValue() && !int.TryParse(optionSkip.Value(), out skip))
                    {
                        throw new CommandParsingException(app, $"Unable to parse [skip] value of '{optionSkip.Value()}'");
                    }

                    var take = 20;
                    if (optionTake.HasValue() && !int.TryParse(optionTake.Value(), out take))
                    {
                        throw new CommandParsingException(app, $"Unable to parse [take] value of '{optionTake.Value()}'");
                    }

                    commandLineOptions.Command = showCommandOptions.ListCommand(startsWith, skip, take);
                });
        }

        public class Display<T> : ICommand
        {
            private readonly Func<CommandContext, Task<T>> query;

            public Display(Func<CommandContext, Task<T>> query)
            {
                this.query = query;
            }

            public async Task ExecuteAsync(CommandContext context)
            {
                var result = await this.query(context).ConfigureAwait(false);
                var json = JsonConvert.SerializeObject(result);
                context.Reporter.Output(json);
            }
        }

        public class List<T> : ICommand
        {
            private readonly string type;
            private readonly Func<CommandContext, Task<ResourceSet<T>>> query;
            private readonly collections.List<string> columnNames = new collections.List<string>();
            private readonly collections.List<Func<T, string>> columns = new collections.List<Func<T, string>>();

            public List(string type, Func<CommandContext, Task<ResourceSet<T>>> query, (string, Func<T, string>) column1, params (string, Func<T, string>)[] columns)
            {
                this.type = type;
                this.query = query;
                this.columnNames.Add(column1.Item1);
                this.columnNames.AddRange(columns.Select(tuple => tuple.Item1));
                this.columns.Add(column1.Item2);
                this.columns.AddRange(columns.Select(tuple => tuple.Item2));
            }

            public async Task ExecuteAsync(CommandContext context)
            {
                var results = await this.query(context).ConfigureAwait(false);
                if (!results.Any())
                {
                    context.Reporter.QueryReturnedNoResults(results, this.type);
                    return;
                }

                // NOTE (Cameron): Madness.
                var format = " ";
                foreach (var column in this.columns.Take(this.columns.Count - 1))
                {
                    var index = this.columns.IndexOf(column);
                    format += string.Format(
                        CultureInfo.InvariantCulture,
                        $"{{{{{index}, -{{0}}}}}}",
                        Math.Max(results.Max(result => column(result)?.Length ?? 0), this.columnNames[index].Length) + 2);
                }

                format += $"{{{this.columns.Count - 1}}}";

                await context.Console.Out.WriteLineAsync().ConfigureAwait(false);

                context.Console.ForegroundColor = ConsoleColor.White;
                context.Console.Out.WriteLine(format, this.columnNames.ToArray());
                context.Console.ResetColor();

                foreach (var user in results)
                {
                    context.Console.Out.WriteLine(format, this.columns.Select(column => column(user)).ToArray());
                }

                await context.Console.Out.WriteLineAsync().ConfigureAwait(false);
                await context.Console.Out.WriteLineAsync($"Showing from {results.Start + 1} to {results.Start + results.Size} of {results.TotalSize} {this.type} in total.")
                    .ConfigureAwait(false);
            }
        }
    }
}
