using HarmonyLib;

namespace ChatCommands
{
    internal static class Patches
    {
        //   Anti Bepinex detection (Thanks o7Moon: https://github.com/o7Moon/CrabGame.AntiAntiBepinex)
        [HarmonyPatch(typeof(EffectManager), nameof(EffectManager.Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0))] // Ensures effectSeed is never set to 4200069 (if it is, modding has been detected)
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.Method_Private_Void_0))] // Ensures connectedToSteam stays false (true means modding has been detected)
        // [HarmonyPatch(typeof(SnowSpeedModdingDetector), nameof(SnowSpeedModdingDetector.Method_Private_Void_0))] // Would ensure snowSpeed is never set to Vector3.zero (though it is immediately set back to Vector3.one due to an accident on Dani's part lol)
        [HarmonyPrefix]
        internal static bool PreBepinexDetection()
            => false;

        
        //   Chat Command Handler
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        internal static bool PreServerSendSendChatMessage(ulong param_0, string param_1, ref BaseCommandResponse __state)
        {
            if (!SteamManager.Instance.IsLobbyOwner() || param_0 <= 1 || !param_1.StartsWith(Api.CommandPrefix))
                return true;

            __state = Api.HandleInput(Api.DefaultExecutionMethod, param_0, param_1[Api.CommandPrefix.Length..]);
            return __state.CommandResponseType != CommandResponseType.Hidden;
        }
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        internal static void PostServerSendSendChatMessage(ulong param_0, BaseCommandResponse __state)
        {
            if (__state != null)
                Api.DefaultExecutionMethod.SendResponse(param_0, __state);
        }

        /*
        //   Command Argument Suggestions
        [HarmonyPatch(typeof(GameUiChatBox), nameof(GameUiChatBox.Update))]
        [HarmonyPostfix]
        internal static void PostGameUiChatBoxUpdate()
            => CommandSugestions.Update();

        [HarmonyPatch(typeof(GameUiChatBox), nameof(GameUiChatBox.SendMessage))]
        [HarmonyPrefix]
        internal static void PreGameUiChatBoxSendMessage(ref string param_1)
        {
            int suggestionPosition = param_1.IndexOf(CommandSugestions.suggestionColor);
            if (suggestionPosition == -1)
                suggestionPosition = param_1.Length;

            string message = param_1[..suggestionPosition].Replace(CommandSugestions.invalidColor, string.Empty).Replace(CommandSugestions.endColor, string.Empty);
            if (message.StartsWith(Api.CommandPrefix))
                param_1 = message;
        }
        */
    }
}