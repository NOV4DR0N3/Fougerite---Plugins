using Facepunch.Clocks.Counters;
using Fougerite;
using RustProto;
using RustProto.Helpers;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using JsonC;
namespace PermissionsManager
{
    public class Configuration
    {
        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(PermissionModule.folderPlugin, fileName);
        }
        public static T ReadyConfigChecked<T>(T obj, string pathFile)
        {
            try
            {
                if (File.Exists(GetAbsoluteFilePath(pathFile)))
                {
                    return JsonHelper.ReadyFile<T>(GetAbsoluteFilePath(pathFile));
                }
                else
                {
                    JsonHelper.SaveFile(obj, GetAbsoluteFilePath(pathFile));
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Falha ao ler path: " + pathFile + "Error: " + ex);
                return default(T);
            }
        }
        public static void CreateFolderCheck(List<string> folders)
        {

            foreach (string folder in folders)
            {
                if (!Directory.Exists(GetAbsoluteFilePath(folder)))
                {
                    Directory.CreateDirectory(GetAbsoluteFilePath(folder));
                }
            }

        }
        public static void carregarConfigs()
        {
            PermissionsManager.PermissionModule.player_permissions = Configuration.ReadyConfigChecked<Dictionary<ulong, PermissionModule.PlayersPermissions>>(PermissionsManager.PermissionModule.player_permissions, "data/permissions_list.json");
            PermissionsManager.PermissionModule.groupLista = Configuration.ReadyConfigChecked<Dictionary<string, PermissionsManager.PermissionModule.Groups>>(PermissionsManager.PermissionModule.groupLista, "data/groups_list.json");
        }
        public static void salvarConfigs()
        {
            if (PermissionsManager.PermissionModule.groupLista.Count != 0)
            {
                Logger.Log("Saving group_lists.");
                JsonHelper.SaveFile(PermissionsManager.PermissionModule.groupLista, Configuration.GetAbsoluteFilePath("data/groups_list.json"));
            }
            if (PermissionsManager.PermissionModule.player_permissions.Count != 0)
            {
                Logger.Log("Saving permissions_list.");
                JsonHelper.SaveFile(PermissionsManager.PermissionModule.player_permissions, Configuration.GetAbsoluteFilePath("data/permissions_list.json"));
            }
        }
    }
}

