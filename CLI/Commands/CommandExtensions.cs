using System.CommandLine;

namespace CLI.Commands;

internal static class CommandExtensions
{
    extension(Command command)
    {
        internal Option<T> AddOption<T>(string name, string alias)
        {
            var option = new Option<T>(name, alias);
            command.Options.Add(option);
            return option;
        }

        internal Option<T> AddRequiredOption<T>(string name, string alias)
        {
            var option = new Option<T>(name, alias) { Required = true };
            command.Options.Add(option);
            return option;
        }

        internal Argument<T> AddArgument<T>(string name, string? description = null)
        {
            var arg = new Argument<T>(name) { Description = description ?? string.Empty };
            command.Arguments.Add(arg);
            return arg;
        }
    }
}
