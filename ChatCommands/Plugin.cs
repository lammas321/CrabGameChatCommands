using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Globalization;

namespace ChatCommands
{
    [BepInPlugin($"lammas123.{MyPluginInfo.PLUGIN_NAME}", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("lammas123.PersistentData", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("lammas123.PermissionGroups", BepInDependency.DependencyFlags.SoftDependency)]
    public class ChatCommands : BasePlugin
    {
        internal static ChatCommands Instance;

        public override void Load()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            Instance = this;

            Api.CommandPrefix = Config.Bind("Chat Commands", "CommandPrefix", "!", "The prefix to use for chat commands.").Value.ToLower();
            Api.CommandArgumentParser = new();

            Api.RegisterCommand(new HelpCommand());
            if (PersistentDataCompatibility.Enabled)
                Api.RegisterCommand(new SetClientDataCommand());

            Api.RegisterExecutionMethod(Api.DefaultExecutionMethod = new ChatExecutionMethod());

            Harmony.CreateAndPatchAll(typeof(Patches));
            Log.LogInfo($"Loaded [{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION}]");
        }
    }
}