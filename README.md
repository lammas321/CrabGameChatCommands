# CrabGameChatCommands
A BepInEx mod for Crab Game that adds functionality for chat commands.

## What does this do?
For the simple answer, it allows for other mods to add chat commands in a neat way without them clashing with each other.

For a more technical behind the scenes answer:
- It makes how commands are formatted and parsed more consistent all around.
- It allows for multiple "execution methods", meaning these commands can be sent from outside of Crab Game, parsed, executed properly and return valid responses, such as through a Discord bot.
- It *can* act as a plug and play chat command system for several different games (if set up properly) with how disconnected I've made it from Crab Game itself.

There are also plans for command suggestions/autocomplete in the future, you can some of the code for it in the repository, however it is buggy in its current state and not a priority of mine currently, thus it is disabled for now.

## Where's all the commands?
This mod by itself only has a help command, you'll need to get other mods, such as [GameMaster](https://github.com/lammas321/CrabGameGameMaster), RightToBearArms, or Overseer to get more fun/useful commands.

## Why can only the host use commands?
If you'd like to give other players permission to use commands, you'll need to use the [PermissionGroups](https://github.com/lammas321/CrabGamePermissionGroups) mod.

## I don't like using an exclamation mark as a command prefix
That can be configured to anything you want in the "BepInEx/config/lammas123.ChatCommands.cfg" config file, however the command prefix is determined by the lobby owner, so you won't be able to use your custom prefix if you're not the host.
