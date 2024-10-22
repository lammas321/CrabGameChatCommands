using BepInEx.IL2CPP;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ChatCommands
{
    internal static class PersistentDataCompatibility
    {
        internal static bool? enabled;
        internal static bool Enabled
            => enabled == null ? (bool)(enabled = IL2CPPChainloader.Instance.Plugins.ContainsKey("lammas123.PersistentData")) : enabled.Value;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static HashSet<ulong> GetPersistentClientDataIds()
            => PersistentData.Api.PersistentClientDataIds;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool SetClientData(ulong clientId, string key, string value)
        {
            PersistentData.ClientDataFile file = PersistentData.Api.GetClientDataFile(clientId);
            bool valid = file.Set(key, value);
            file.SaveFile();
            return valid;
        }
    }

    internal static class PermissionGroupsCompatibility
    {
        internal static bool? enabled;
        internal static bool Enabled
            => enabled == null ? (bool)(enabled = IL2CPPChainloader.Instance.Plugins.ContainsKey("lammas123.PermissionGroups")) : enabled.Value;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool PermissionGroupHasPermission(string permissionGroupId, string permission)
            => PermissionGroups.Api.PermissionGroupHasPermission(permissionGroupId, permission);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static string GetClientPermissionGroup(ulong clientId)
            => PermissionGroups.Api.GetClientPermissionGroup(clientId);
    }
}