using System.Collections.Generic;
using static ChatCommands.CommandArgumentParser;

namespace ChatCommands
{
    public static class Api
    {
        internal static Dictionary<string, BaseCommand> commands = [];
        internal static Dictionary<string, BaseExecutionMethod> executionMethods = [];

        public static string CommandPrefix { get; internal set; }
        public static BaseExecutionMethod DefaultExecutionMethod { get; internal set; }

        public static CommandArgumentParser CommandArgumentParser { get; internal set; }

        public static bool RegisterCommand(BaseCommand command)
        {
            if (HasCommand(command.Id))
                return false;

            commands.Add(command.Id, command);
            return true;
        }
        public static bool HasCommand(string commandId)
            => commands.ContainsKey(commandId);
        public static BaseCommand GetCommand(string commandId)
            => HasCommand(commandId) ? commands[commandId] : null;
        public static BaseCommand[] GetCommands()
            => [.. commands.Values];

        public static bool RegisterExecutionMethod(BaseExecutionMethod executionMethod)
        {
            if (HasExecutionMethod(executionMethod.Id))
                return false;

            executionMethods.Add(executionMethod.Id, executionMethod);
            return true;
        }
        public static bool HasExecutionMethod(string executionMethodId)
            => executionMethods.ContainsKey(executionMethodId);
        public static BaseExecutionMethod GetExecutionMethod(string executionMethodId)
            => HasExecutionMethod(executionMethodId) ? executionMethods[executionMethodId] : null;
        public static BaseExecutionMethod[] GetExecutionMethods()
            => [.. executionMethods.Values];

        public static BaseCommandResponse HandleInput(BaseExecutionMethod executionMethod, object executorDetails, string input, bool ignorePermissions = false)
        {
            ParsedResult<BaseCommand> result = CommandArgumentParser.Parse<BaseCommand>(input);
            if (!result.successful || (!ignorePermissions && !executionMethod.HasPermission(executorDetails, $"command.{result.result.Id}")))
                return new BasicCommandResponse([$"'{result.parsedArg}' is not a command."], CommandResponseType.Private);

            return result.result.Execute(executionMethod, executorDetails, result.newArgs, ignorePermissions);
        }
    }
}