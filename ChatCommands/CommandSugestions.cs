using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChatCommands.CommandArgumentParser;

// Could use massive improvement and fixes
namespace ChatCommands
{
    public static class CommandSugestions
    {
        internal static readonly string suggestionColor = "<color=#444444>";
        internal static readonly string invalidColor = "<color=#FF0000>";
        internal static readonly string endColor = "</color>";

        internal static string previousText = string.Empty;
        internal static string previousInputText = string.Empty;
        internal static int previousStringPosition = 0;
        internal static string previousOption = string.Empty;

        internal static void SetText(string validText, string extraText, bool extraTextInvald, string suggestionText)
        {
            if (extraTextInvald)
                extraText = $"{invalidColor}{extraText}{endColor}";
            if (suggestionText.Length != 0)
                suggestionText = $"{suggestionColor}{suggestionText}";
            GameUiChatBox.Instance.inputField.text = $"{validText}{extraText}{suggestionText}";
        }
        
        internal static void Update()
        {
            int suggestionPosition = GameUiChatBox.Instance.inputField.text.IndexOf(suggestionColor);
            if (suggestionPosition == -1)
                suggestionPosition = GameUiChatBox.Instance.inputField.text.Length;
            else if (GameUiChatBox.Instance.inputField.stringPosition > suggestionPosition) // Limit string position to before suggestions
                GameUiChatBox.Instance.inputField.stringPosition = suggestionPosition;

            string inputText = GameUiChatBox.Instance.inputField.text[..suggestionPosition].Replace(invalidColor, string.Empty).Replace(endColor, string.Empty);
            int stringPosition = GameUiChatBox.Instance.inputField.stringPosition;
            bool wasCommand = previousInputText.StartsWith(Api.CommandPrefix);

            if (!inputText.StartsWith(Api.CommandPrefix))
            {
                // Save resources, do nothing if the input field is not being used for a command
                if (GameUiChatBox.Instance.inputField.text != previousText)
                    previousText = GameUiChatBox.Instance.inputField.text;
                if (inputText != previousInputText)
                    previousInputText = inputText;
                if (stringPosition != previousStringPosition)
                    previousStringPosition = stringPosition;

                if (!wasCommand)
                    return;

                // Was a command last frame
                GameUiChatBox.Instance.inputField.text = inputText;
                stringPosition = GameUiChatBox.Instance.inputField.text.Length;
                GameUiChatBox.Instance.inputField.stringPosition = stringPosition;
                previousStringPosition = stringPosition;
                previousOption = string.Empty;
                return;
            }

            bool downKeyPressed = Input.GetKeyDown(KeyCode.DownArrow);
            bool upKeyPressed = Input.GetKeyDown(KeyCode.UpArrow);
            bool fillKeyPressed = Input.GetKeyDown(KeyCode.Tab);
            if (GameUiChatBox.Instance.inputField.text == previousText && !(downKeyPressed || upKeyPressed || fillKeyPressed))
            {
                // Save resources, do nothing if the input text hasn't changed and none of the keybinds have been pressed this frame
                previousStringPosition = stringPosition;
                return;
            }

            //ChatCommands.Instance.Log.LogInfo($"Text Update: '{previousText}' -> '{GameUiChatBox.Instance.inputField.text}'");

            string validText = string.Empty;
            string extraText = string.Empty;
            bool extraTextInvalid = false;
            int parsedArgLength = 0;
            
            BaseCommand command = null;
            string[] options = [];
            int optionIndex = 0;

            ParsedResult<BaseCommand> commandResult = Api.CommandArgumentParser.Parse<BaseCommand>(inputText[Api.CommandPrefix.Length..]);
            if (commandResult.successful)
            {
                string args = commandResult.newArgs;
                validText = $"{Api.CommandPrefix}{commandResult.parsedArg}";
                command = commandResult.result;

                //ChatCommands.Instance.Log.LogInfo($"Found command successfully: '{command.Id}'");

                if (command.Id.Length != inputText.Length - Api.CommandPrefix.Length)
                {
                    // Parse command args
                    for (int argIndex = 0; argIndex < command.Args.args.Length; argIndex++)
                    {
                        // Go through all args and get their options
                        CommandArgument arg = command.Args.args[argIndex];
                        OptionsResult result = default;
                        List<string> argOptions = [];
                        string newArgs = null;
                        string parsedArg = null;
                        foreach (Type type in arg.types)
                        {
                            result = Api.CommandArgumentParser.GenericOptions(type, args);
                            argOptions.AddRange(result.options);
                            if (newArgs == null && result.valid)
                            {
                                newArgs = result.newArgs;
                                parsedArg = result.parsedArg;
                            }
                        }

                        if (newArgs == null)
                        {
                            // Nothing was a valid arg, show that the input was invalid
                            //ChatCommands.Instance.Log.LogInfo($"Invalid arg #{argIndex + 1}: '{result.parsedArg}'");

                            extraText = $" {args}";
                            extraTextInvalid = true;
                            if (argIndex == command.Args.args.Length - 1 || result.newArgs.Length == 0)
                            {
                                // No other args or no extra text comes after this, get options
                                options = [.. argOptions];
                                optionIndex = Math.Max(0, Array.IndexOf(options, previousOption));
                                parsedArgLength = result.parsedArg.Length;
                            }
                            args = result.newArgs;
                            break;
                        }

                        //ChatCommands.Instance.Log.LogInfo($"Valid arg #{argIndex + 1}: '{result.parsedArg}'");

                        validText += $" {parsedArg}";
                        if (args.Length == 0)
                        {
                            // There was no more text to parse, but there being no text was still valid, get options
                            options = [.. argOptions];
                            optionIndex = Math.Max(0, Array.IndexOf(options, previousOption));
                            parsedArgLength = parsedArg.Length;
                            args = newArgs;
                            break;
                        }
                        args = newArgs;
                    }
                }
            }
            else
            {
                // Invalid command
                //ChatCommands.Instance.Log.LogInfo($"Unable to find command: '{commandResult.parsedArg}'");

                extraText = inputText;
                extraTextInvalid = true;
                options = commandResult.newArgs.Length == 0 ? [.. Api.GetCommands().Select(command => command.Id).Where(option => option.StartsWith(commandResult.parsedArg))] : [];
                optionIndex = Math.Max(0, Array.IndexOf(options, previousOption));
                parsedArgLength = commandResult.parsedArg.Length;
            }

            //ChatCommands.Instance.Log.LogInfo($"Options ({options.Length}):");
            //foreach (string option in options)
            //    ChatCommands.Instance.Log.LogInfo($"- '{option}'");

            if (options.Length != 0)
            {
                if (fillKeyPressed)
                {
                    ChatCommands.Instance.Log.LogInfo("Fill key pressed");
                    if (options[optionIndex][parsedArgLength..].Length != 0)
                    {
                        //ChatCommands.Instance.Log.LogInfo("Filling in suggested text and setting result text");
                        // Fill in suggested text
                        extraText += options[optionIndex][parsedArgLength..];
                        previousText = GameUiChatBox.Instance.inputField.text;
                        SetText(validText, extraText, extraTextInvalid, string.Empty);
                        previousInputText = inputText;
                        stringPosition = GameUiChatBox.Instance.inputField.text.Length;
                        GameUiChatBox.Instance.inputField.stringPosition = stringPosition;
                        previousStringPosition = stringPosition;
                        previousOption = options[optionIndex];
                        return;
                    }
                }
                else if (downKeyPressed)
                {
                    //ChatCommands.Instance.Log.LogInfo("Down key pressed");
                    // Prevents caret from jumping to the end of the text and increments the option index
                    GameUiChatBox.Instance.inputField.stringPosition = previousStringPosition;
                    optionIndex = Math.Clamp(++optionIndex, 0, options.Length - 1);
                }
                else if (upKeyPressed)
                {
                    //ChatCommands.Instance.Log.LogInfo("Up key pressed");
                    // Prevents caret from jumping to the start of the text and decrements the option index
                    GameUiChatBox.Instance.inputField.stringPosition = previousStringPosition;
                    optionIndex = Math.Clamp(--optionIndex, 0, options.Length - 1);
                }
            }

            //ChatCommands.Instance.Log.LogInfo("Setting result text");
            previousText = GameUiChatBox.Instance.inputField.text;
            SetText(validText, extraText, extraTextInvalid, options.Length == 0 ? string.Empty : options[optionIndex][parsedArgLength..]);
            previousInputText = inputText;
            stringPosition = GameUiChatBox.Instance.inputField.text.IndexOf(suggestionColor);
            if (stringPosition == -1)
                stringPosition = GameUiChatBox.Instance.inputField.text.Length;
            GameUiChatBox.Instance.inputField.stringPosition = stringPosition;
            previousStringPosition = stringPosition;
            previousOption = options.Length == 0 ? string.Empty : options[optionIndex];
        }
    }
}
