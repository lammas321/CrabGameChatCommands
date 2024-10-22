using System;
using System.Collections.Generic;
using System.Linq;
using static ChatCommands.CommandArgumentParser;

namespace ChatCommands
{
    public readonly struct CommandArgument(Type[] types, string display = null, bool required = false)
    {
        public readonly Type[] types = types;
        public readonly string display = display;
        public readonly bool required = required;

        public override string ToString()
            => $"{(required ? "<" : "[")}{display}{(required ? ">" : "]")}";
    }

    public readonly struct CommandArguments(CommandArgument[] args)
    {
        public readonly CommandArgument[] args = args;

        public override string ToString()
        {
            string[] args = new string[this.args.Length];
            for (int i = 0; i < args.Length; i++)
                args[i] = this.args[i].ToString();
            return string.Join(" ", args);
        }
    }


    public enum CommandResponseType
    {
        Public,
        Private,
        Hidden
    }
    
    public abstract class BaseCommandResponse
    {
        protected Dictionary<string, object> data = [];

        public BaseCommandResponse(CommandResponseType commandResponseType = CommandResponseType.Public)
            => data.Add("commandResponseType", commandResponseType);

        public abstract string[] GetFormattedResponse();

        public T Get<T>(string key)
            => data.TryGetValue(key, out object value) ? (T)value : default;

        public CommandResponseType CommandResponseType
        {
            get => Get<CommandResponseType>("commandResponseType");
        }
    }
    public class BasicCommandResponse : BaseCommandResponse
    {
        public BasicCommandResponse(string[] lines, CommandResponseType commandResponseType = CommandResponseType.Public) : base(commandResponseType)
            => data.Add("lines", lines);

        public override string[] GetFormattedResponse()
            => Get<string[]>("lines");
    }
    public class StyledCommandResponse : BasicCommandResponse
    {
        public StyledCommandResponse(string title, string[] lines, CommandResponseType commandResponseType = CommandResponseType.Public, bool onlyTitleFirstLine = true) : base(lines, commandResponseType)
        {
            data.Add("title", title);
            data.Add("onlyTitleFirstLine", onlyTitleFirstLine);
        }

        public override string[] GetFormattedResponse()
            => [Get<string>("title"), .. Get<string[]>("lines")];

        public string Title
        {
            get => Get<string>("title");
        }

        public bool OnlyTitleFirstLine
        {
            get => Get<bool>("onlyTitleFirstLine");
        }
    }


    public abstract class BaseCommand
    {
        protected string id;
        protected string description;
        protected CommandArguments args;
        
        public string Id
            => id;
        public string Description
            => description;
        public CommandArguments Args
            => args;
        
        public abstract BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false);
    }

    public class HelpCommand : BaseCommand
    {
        public HelpCommand()
        {
            id = "help";
            description = "View helpful information about available commands.";
            args = new([
                new(
                    [typeof(BaseCommand), typeof(int)],
                    "command/page"
                )
            ]);
        }

        public BaseCommandResponse ShowHelpPage(BaseExecutionMethod executionMethod, object executorDetails, int page = 1, bool ignorePermissions = false)
        {
            if (page < 1)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);

            int currentPage = 1;
            int currentLine = 0;
            List<string> lines = [string.Empty];

            BaseCommand[] commands = Api.GetCommands();
            for (int i = 0; i < commands.Length; i++)
            {
                BaseCommand command = commands[i];
                if (!ignorePermissions && !executionMethod.HasPermission(executorDetails, $"command.{command.Id}"))
                    continue;

                string str = Api.CommandPrefix + command.Id;
                if (i != commands.Length - 1)
                    str += ", ";

                if (lines[currentLine].Length + str.Length - 1 > executionMethod.MaxResponseLength)
                {
                    currentLine++;
                    if (currentLine == 3)
                    {
                        if (currentPage + 1 > page)
                            break;

                        currentPage++;
                        currentLine = 0;
                        lines = [];
                    }

                    lines.Add(string.Empty);
                }

                lines[currentLine] += str;
            }

            if (page != currentPage)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);

            return new StyledCommandResponse($"Help Page #{page}", [.. lines], CommandResponseType.Private);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return ShowHelpPage(executionMethod, executorDetails, 1, ignorePermissions);

            ParsedResult<int> pageResult = Api.CommandArgumentParser.Parse<int>(args);
            if (pageResult.successful)
                return ShowHelpPage(executionMethod, executorDetails, pageResult.result, ignorePermissions);

            ParsedResult<BaseCommand> commandResult = Api.CommandArgumentParser.Parse<BaseCommand>(args);
            if (!commandResult.successful || (!ignorePermissions && !executionMethod.HasPermission(executorDetails, $"command.{commandResult.result.Id}")))
                return new BasicCommandResponse([$"'{args}' is not a command."], CommandResponseType.Private);

            return new StyledCommandResponse("Command Info", [$"!{commandResult.result.Id} {commandResult.result.Args}", commandResult.result.Description], CommandResponseType.Private);
        }
    }

    public class SetClientDataCommand : BaseCommand
    {
        public SetClientDataCommand()
        {
            id = "setclientdata";
            description = "Sets the value of a key for a given player.";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId), typeof(DefaultCommandArgumentParsers.OfflineClientId)],
                    "player",
                    true
                ),
                new(
                    [typeof(string)],
                    "key",
                    true
                ),
                new(
                    [typeof(DefaultCommandArgumentParsers.QuotedString)],
                    "value",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A player is required for the first argument."], CommandResponseType.Hidden);

            ulong clientId;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> onlineClientResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
            if (onlineClientResult.successful)
            {
                clientId = onlineClientResult.result;
                args = onlineClientResult.newArgs;
            }
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OfflineClientId> offlineClientResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OfflineClientId>(args);
                if (offlineClientResult.successful)
                {
                    clientId = offlineClientResult.result;
                    args = offlineClientResult.newArgs;
                }
                else
                    return new BasicCommandResponse(["You did not select any players."], CommandResponseType.Hidden);
            }

            if (args.Length == 0)
                return new BasicCommandResponse(["A key is required for the second argument."], CommandResponseType.Hidden);

            ParsedResult<string> keyResult = Api.CommandArgumentParser.Parse<string>(args);
            args = keyResult.newArgs;

            if (args.Length == 0)
                return new BasicCommandResponse(["A value is required for the second argument."], CommandResponseType.Hidden);

            ParsedResult<DefaultCommandArgumentParsers.QuotedString> valueResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.QuotedString>(args);
            if (!valueResult.successful)
                return new BasicCommandResponse(["You did not provide a valid value."], CommandResponseType.Hidden);

            if (!PersistentDataCompatibility.SetClientData(clientId, keyResult.result, valueResult.result))
                return new BasicCommandResponse(["Unable to save that key/value."], CommandResponseType.Hidden);

            return new BasicCommandResponse(["Saved."], CommandResponseType.Hidden);
        }
    }
}