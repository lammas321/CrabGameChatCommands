using SteamworksNative;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static ChatCommands.CommandArgumentParser;
using static ChatCommands.DefaultCommandArgumentParsers;

namespace ChatCommands
{
    public class CommandArgumentParser
    {
        public struct ParsedResult<T>(T result, string parsedArg, string newArgs, bool successful)
        {
            public T result = result;
            public string parsedArg = parsedArg;
            public string newArgs = newArgs;
            public bool successful = successful;
        }
        public struct GenericParsedResult(string parsedArg, string newArgs, bool successful)
        {
            public string parsedArg = parsedArg;
            public string newArgs = newArgs;
            public bool successful = successful;
        }
        public struct OptionsResult(string[] options, string parsedArg, string newArgs, bool valid)
        {
            public string[] options = options;
            public string parsedArg = parsedArg;
            public string newArgs = newArgs;
            public bool valid = valid;
        }

        public delegate object ParserDelegate(ref string args, out string parsedArg, out bool successful);
        public delegate string[] OptionDelegate(ref string args, out string parsedArg, out bool valid);

        internal Dictionary<Type, ParserDelegate> parsers;
        internal Dictionary<Type, OptionDelegate> options;

        internal CommandArgumentParser()
        {
            parsers = [];
            options = [];

            RegisterType(typeof(string), new(ParseString), new(StringOptions));
            RegisterType(typeof(Default), new(ParseDefault), new(DefaultOptions));
            RegisterType(typeof(Reset), new(ParseReset), new(ResetOptions));
            RegisterType(typeof(QuotedString), new(ParseQuotedString), new(QuotedStringOptions));
            RegisterType(typeof(bool), new(ParseBoolean), new(BooleanOptions));
            RegisterType(typeof(int), new(ParseInt), new(IntOptions));
            RegisterType(typeof(float), new(ParseFloat), new(FloatOptions));
            RegisterType(typeof(Map), new(ParseMap), new(MapOptions));
            RegisterType(typeof(GameModeData), new(ParseGameMode), new(GameModeOptions));
            RegisterType(typeof(ItemData), new(ParseItem), new(ItemOptions));
            RegisterType(typeof(OnlineClientId), new(ParseOnlineClientId), new(OnlineClientIdOptions));
            RegisterType(typeof(OnlineClientId[]), new(ParseOnlineClientIds), new(OnlineClientIdsOptions));
            RegisterType(typeof(OfflineClientId), new(ParseOfflineClientId), new(OfflineClientIdOptions));
            RegisterType(typeof(BaseCommand), new(ParseCommand), new(CommandOptions));
        }

        public ParsedResult<T> Parse<T>(string args)
        {
            if (!parsers.ContainsKey(typeof(T)))
                return new(default, default, args, false);

            return new((T)parsers[typeof(T)](ref args, out string parsedArg, out bool successful), parsedArg, args, successful);
        }
        public GenericParsedResult GenericParse(Type type, string args)
        {
            if (!parsers.ContainsKey(type))
                return new(default, args, false);

            parsers[type](ref args, out string parsedArg, out bool successful);
            return new(parsedArg, args, successful);
        }
        public OptionsResult Options<T>(string args)
        {
            if (!options.ContainsKey(typeof(T)))
                return new(default, default, args, false);

            return new(options[typeof(T)](ref args, out string parsedArg, out bool valid), parsedArg, args, valid);
        }
        public OptionsResult GenericOptions(Type type, string args)
        {
            if (!options.ContainsKey(type))
                return new(default, default, args, false);

            return new(options[type](ref args, out string parsedArg, out bool valid), parsedArg, args, valid);
        }


        public bool RegisterType(Type type, ParserDelegate parserDelegate, OptionDelegate optionDelegate)
        {
            if (HasType(type))
                return false;

            parsers.Add(type, parserDelegate);
            options.Add(type, optionDelegate);
            return true;
        }
        public bool HasType(Type type)
            => parsers.ContainsKey(type);
        public Type[] GetTypes()
            => [.. parsers.Keys];
    }
    
    public static class DefaultCommandArgumentParsers
    {
        public struct Default();
        public struct Reset();
        public struct QuotedString(string qs)
        {
            public string qs = qs;
            public static implicit operator string(QuotedString qs) => qs.qs;
        }
        public struct OnlineClientId(ulong clientId)
        {
            public ulong clientId = clientId;
            public static implicit operator ulong(OnlineClientId onlineClientId) => onlineClientId.clientId;
        }
        public struct OfflineClientId(ulong clientId)
        {
            public ulong clientId = clientId;
            public static implicit operator ulong(OfflineClientId offlineClientId) => offlineClientId.clientId;
        }

        public static object ParseString(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(string);
            }

            int split = args.IndexOf(' ');
            if (split == -1)
                split = args.Length;

            string arg = args[..split];
            args = args[Math.Min(split + 1, args.Length)..];
            parsedArg = arg;
            successful = true;
            return arg;
        }
        public static string[] StringOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [];
        }

        public static object ParseDefault(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(bool);
            }

            args = result.newArgs;
            parsedArg = result.parsedArg;
            successful = result.result == "default";
            return new Default();
        }
        public static string[] DefaultOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<Default> result = Api.CommandArgumentParser.Parse<Default>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return ["default"];
        }

        public static object ParseReset(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(bool);
            }

            args = result.newArgs;
            parsedArg = result.parsedArg;
            successful = result.result == "reset";
            return new Reset();
        }
        public static string[] ResetOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<Reset> result = Api.CommandArgumentParser.Parse<Reset>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return ["reset"];
        }
        
        public static object ParseQuotedString(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(QuotedString);
            }

            string[] strings = args.Split(' ');
            int end = 1;
            string value;
            if (strings[0].StartsWith('"'))
            {
                strings[0] = strings[0][1..];
                foreach (string str in strings)
                {
                    if (str.EndsWith('"'))
                        break;
                    end++;
                }

                value = string.Join(' ', strings[..end]);
                if (value.EndsWith('"'))
                    value = value[..(value.Length - 1)];
            }
            else
                value = strings[0];

            args = string.Join(' ', strings[end..]);
            parsedArg = value;
            successful = true;
            return new QuotedString(value);
        }
        public static string[] QuotedStringOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<QuotedString> result = Api.CommandArgumentParser.Parse<QuotedString>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [];
        }

        public static object ParseBoolean(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(bool);
            }

            bool value = "true".StartsWith(result.result);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            successful = value || "false".StartsWith(result.result);
            return value;
        }
        public static string[] BooleanOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<bool> result = Api.CommandArgumentParser.Parse<bool>(args);

            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [.. new string[] { "true", "false" }.Where(str => str.StartsWith(result.parsedArg))];
        }

        public static object ParseInt(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(int);
            }

            args = result.newArgs;
            parsedArg = result.result;
            successful = int.TryParse(result.result, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture.NumberFormat, out int value);
            return value;
        }
        public static string[] IntOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<int> result = Api.CommandArgumentParser.Parse<int>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [];
        }

        public static object ParseFloat(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(float);
            }

            args = result.newArgs;
            parsedArg = result.result;
            successful = float.TryParse(result.result, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out float value);
            return value;
        }
        public static string[] FloatOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<float> result = Api.CommandArgumentParser.Parse<float>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [];
        }

        public static object ParseMap(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(Map);
            }

            ParsedResult<int> mapIdResult = Api.CommandArgumentParser.Parse<int>(args);
            successful = mapIdResult.successful;
            if (successful)
            {
                args = mapIdResult.newArgs;
                if (mapIdResult.result < 0 || mapIdResult.result >= MapManager.Instance.maps.Count)
                {
                    parsedArg = string.Empty;
                    successful = false;
                    return default(Map);
                }

                parsedArg = mapIdResult.parsedArg;
                successful = true;
                return MapManager.Instance.maps[mapIdResult.result];
            }

            ParsedResult<string> mapNameResult = Api.CommandArgumentParser.Parse<string>(args);
            mapNameResult.result = mapNameResult.result.ToLower();
            args = mapNameResult.newArgs;
            parsedArg = mapNameResult.parsedArg;
            successful = mapNameResult.successful;
            if (successful)
            {
                foreach (Map map in MapManager.Instance.maps.OrderBy(map => map.mapName))
                    if (map.mapName.Replace(" ", string.Empty).ToLower().StartsWith(mapNameResult.result))
                        return map;

                successful = false;
            }

            return default(Map);
        }
        public static string[] MapOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<Map> result = Api.CommandArgumentParser.Parse<Map>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [.. MapManager.Instance.maps.Select(map => map.mapName.Replace(" ", string.Empty).ToLower()).Where(mapName => mapName.StartsWith(result.parsedArg)).OrderBy(mapName => mapName)];
        }

        public static object ParseGameMode(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(GameModeData);
            }

            ParsedResult<int> gameModeIdResult = Api.CommandArgumentParser.Parse<int>(args);
            successful = gameModeIdResult.successful;
            if (successful)
            {
                args = gameModeIdResult.newArgs;
                if (gameModeIdResult.result < 0 || gameModeIdResult.result >= GameModeManager.Instance.allGameModes.Count)
                {
                    parsedArg = string.Empty;
                    successful = false;
                    return default(GameModeData);
                }

                parsedArg = gameModeIdResult.parsedArg;
                successful = true;
                return GameModeManager.Instance.allGameModes[gameModeIdResult.result];
            }

            ParsedResult<string> gameModeNameResult = Api.CommandArgumentParser.Parse<string>(args);
            gameModeNameResult.result = gameModeNameResult.result.ToLower();
            args = gameModeNameResult.newArgs;
            parsedArg = gameModeNameResult.parsedArg;
            successful = gameModeNameResult.successful;
            if (successful)
            {
                foreach (GameModeData gameMode in GameModeManager.Instance.allGameModes.OrderBy(gameMode => gameMode.modeName))
                    if (gameMode.modeName.Replace(" ", string.Empty).ToLower().StartsWith(gameModeNameResult.result))
                        return gameMode;

                successful = false;
            }

            return default(GameModeData);
        }
        public static string[] GameModeOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<GameModeData> result = Api.CommandArgumentParser.Parse<GameModeData>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [.. GameModeManager.Instance.allGameModes.Select(gameMode => gameMode.modeName.Replace(" ", string.Empty).ToLower()).Where(gameModeName => gameModeName.StartsWith(result.parsedArg)).OrderBy(gameModeName => gameModeName)];
        }

        public static object ParseItem(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(ItemData);
            }

            ParsedResult<int> itemIdResult = Api.CommandArgumentParser.Parse<int>(args);
            successful = itemIdResult.successful;
            if (successful)
            {
                args = itemIdResult.newArgs;
                if (itemIdResult.result < 0 || itemIdResult.result >= ItemManager.idToItem.Count)
                {
                    parsedArg = string.Empty;
                    successful = false;
                    return default(ItemData);
                }

                parsedArg = itemIdResult.parsedArg;
                successful = true;
                return ItemManager.idToItem[itemIdResult.result];
            }

            ParsedResult<string> itemNameResult = Api.CommandArgumentParser.Parse<string>(args);
            itemNameResult.result = itemNameResult.result.ToLower();
            args = itemNameResult.newArgs;
            parsedArg = itemNameResult.parsedArg;
            successful = itemNameResult.successful;
            if (successful)
            {
                List<ItemData> items = [.. ItemManager.idToItem.Values];
                foreach (ItemData item in items.OrderBy(item => item.itemName))
                    if (item.itemName.Replace(" ", string.Empty).ToLower().StartsWith(itemNameResult.result))
                        return item;

                successful = false;
            }

            return default(ItemData);
        }
        public static string[] ItemOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<ItemData> result = Api.CommandArgumentParser.Parse<ItemData>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            List<ItemData> items = [.. ItemManager.idToItem.Values];
            return [.. items.Select(item => item.itemName.Replace(" ", string.Empty).ToLower()).Where(itemName => itemName.StartsWith(result.parsedArg)).OrderBy(itemName => itemName)];
        }

        public static object ParseOnlineClientId(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(OnlineClientId);
            }

            if (args.StartsWith('@'))
            {
                ParsedResult<string> onlineClientIdResult = Api.CommandArgumentParser.Parse<string>(args);
                args = onlineClientIdResult.newArgs;
                parsedArg = onlineClientIdResult.parsedArg;
                if (onlineClientIdResult.successful)
                    return new OnlineClientId((successful = ulong.TryParse(onlineClientIdResult.result[1..], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out ulong value))
                        ? value
                        : default);

                successful = false;
                return default(OnlineClientId);
            }

            if (args.StartsWith('#'))
            {
                ParsedResult<int> playerNumberResult = Api.CommandArgumentParser.Parse<int>(args[1..]);
                args = playerNumberResult.newArgs;
                parsedArg = playerNumberResult.parsedArg;
                if (playerNumberResult.successful)
                    return new OnlineClientId((successful = playerNumberResult.result >= 1 && playerNumberResult.result <= LobbyManager.Instance.field_Private_ArrayOf_UInt64_0.Length)
                        ? LobbyManager.Instance.field_Private_ArrayOf_UInt64_0[playerNumberResult.result - 1]
                        : default);

                successful = false;
                return default(OnlineClientId);
            }

            ParsedResult<QuotedString> playerNameResult = Api.CommandArgumentParser.Parse<QuotedString>(args);
            args = playerNameResult.newArgs;
            parsedArg = playerNameResult.parsedArg;
            if (playerNameResult.successful)
            {
                ulong foundOnlineClientId = default;
                foreach (ulong onlineClientId in LobbyManager.steamIdToUID.Keys)
                {
                    string username = SteamFriends.GetFriendPersonaName(new(onlineClientId));
                    if (username.StartsWith(playerNameResult.result))
                        foundOnlineClientId = onlineClientId;

                    if (username == playerNameResult.result)
                    {
                        foundOnlineClientId = onlineClientId;
                        break;
                    }
                }

                if (foundOnlineClientId != default)
                {
                    successful = true;
                    return new OnlineClientId(foundOnlineClientId);
                }
            }

            successful = false;
            return default(OnlineClientId);
        }
        public static string[] OnlineClientIdOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<OnlineClientId> result = Api.CommandArgumentParser.Parse<OnlineClientId>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            int playerCount = LobbyManager.steamIdToUID.Count;
            string[] options = new string[playerCount * 3];
            int i = 0;
            foreach (ulong clientId in LobbyManager.steamIdToUID.Keys)
            {
                string username = SteamFriends.GetFriendPersonaName(new(clientId));
                options[i] = username.Contains(' ') || username.Contains('"') || username.StartsWith('@') || username.StartsWith('#') ? $"\"{username}\"" : username;
                options[playerCount + i] = $"#{LobbyManager.steamIdToUID[clientId] + 1}";
                options[playerCount * 2 + i++] = $"@{clientId}";
            }
            return [.. options.Where(str => str.StartsWith(result.parsedArg))];
        }

        public static object ParseOnlineClientIds(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0 || !args.StartsWith('*'))
            {
                parsedArg = string.Empty;
                successful = false;
                return default(OnlineClientId[]);
            }

            ParsedResult<string> selectorResult = Api.CommandArgumentParser.Parse<string>(args);
            args = selectorResult.newArgs;
            parsedArg = selectorResult.parsedArg;
            if (selectorResult.successful)
            {
                IEnumerable<ulong> clientIds = [.. LobbyManager.steamIdToUID.Keys];
                if (parsedArg == "*")
                {
                    successful = true;
                    return clientIds.Where(clientId => GameManager.Instance.activePlayers.ContainsKey(clientId) && !GameManager.Instance.activePlayers[clientId].dead).Select(clientId => new OnlineClientId(clientId)).ToArray();
                }
                if (parsedArg == "*d")
                {
                    successful = true;
                    return clientIds.Where(clientId => !GameManager.Instance.activePlayers.ContainsKey(clientId) || GameManager.Instance.activePlayers[clientId].dead).Select(clientId => new OnlineClientId(clientId)).ToArray();
                }
                if (parsedArg == "*e")
                {
                    successful = true;
                    return clientIds.Select(clientId => new OnlineClientId(clientId)).ToArray();
                }
            }

            successful = false;
            return default(OnlineClientId[]);
        }
        public static string[] OnlineClientIdsOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<OnlineClientId[]> result = Api.CommandArgumentParser.Parse<OnlineClientId[]>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return ["*", "*a", "*e"];
        }

        public static object ParseOfflineClientId(ref string args, out string parsedArg, out bool successful)
        {
            if (args.Length == 0 || !args.StartsWith('@'))
            {
                parsedArg = string.Empty;
                successful = false;
                return default(OfflineClientId);
            }
            
            ParsedResult<string> offlineClientIdResult = Api.CommandArgumentParser.Parse<string>(args);
            args = offlineClientIdResult.newArgs;
            parsedArg = offlineClientIdResult.parsedArg;
            if (offlineClientIdResult.successful)
                return new OfflineClientId((successful = ulong.TryParse(offlineClientIdResult.result[1..], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out ulong value))
                    ? value
                    : default);

            successful = false;
            return default(OfflineClientId);
        }
        public static string[] OfflineClientIdOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<OfflineClientId> result = Api.CommandArgumentParser.Parse<OfflineClientId>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;

            if (!PersistentDataCompatibility.Enabled)
                return [];

            int playerCount = PersistentDataCompatibility.GetPersistentClientDataIds().Count;
            string[] options = new string[playerCount];
            int i = 0;
            foreach (ulong clientId in PersistentDataCompatibility.GetPersistentClientDataIds())
                options[i++] = $"@{clientId}";
            return [.. options.Where(str => str.StartsWith(result.parsedArg))];
        }

        public static object ParseCommand(ref string args, out string parsedArg, out bool successful)
        {
            ParsedResult<string> result = Api.CommandArgumentParser.Parse<string>(args);
            if (!result.successful)
            {
                parsedArg = string.Empty;
                successful = false;
                return default(BaseCommand);
            }

            args = result.newArgs;
            parsedArg = result.parsedArg;
            successful = Api.HasCommand(result.result);
            return Api.GetCommand(result.result);
        }
        public static string[] CommandOptions(ref string args, out string parsedArg, out bool valid)
        {
            ParsedResult<BaseCommand> result = Api.CommandArgumentParser.Parse<BaseCommand>(args);
            args = result.newArgs;
            parsedArg = result.parsedArg;
            valid = result.successful;
            return [.. Api.GetCommands().Select(command => command.Id).Where(commandId => commandId.StartsWith(result.parsedArg))];
        }
    }
}