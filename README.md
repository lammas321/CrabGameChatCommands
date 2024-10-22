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
This mod by itself only has a help command, you'll need to get other mods, such as GameMaster, RightToBearArms, or Overseer to get more fun/useful commands.
