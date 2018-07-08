using Facepunch.Clocks.Counters;
using Fougerite;
using RustProto;
using RustProto.Helpers;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace PermissionsManager
{
    public class Configuration
    {
        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(PermissionModule.ConfigssFolder, fileName);
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
            PermissionsManager.PermissionModule.permLista = Configuration.ReadyConfigChecked<Dictionary<ulong, List<string>>>(PermissionsManager.PermissionModule.permLista, "data/permissions_list.json");
            PermissionsManager.PermissionModule.groupLista = Configuration.ReadyConfigChecked<Dictionary<string, PermissionsManager.PermissionModule.Groups>>(PermissionsManager.PermissionModule.groupLista, "data/groups_list.json");
        }
        public static void salvarConfigs()
        {
            if (PermissionsManager.PermissionModule.groupLista.Count != 0)
            {
                Logger.Log("Saving group_lists.");
                JsonHelper.SaveFile(PermissionsManager.PermissionModule.groupLista, Configuration.GetAbsoluteFilePath("data/groups_list.json"));
            }
            if (PermissionsManager.PermissionModule.permLista.Count != 0)
            {
                Logger.Log("Saving permissions_list.");
                JsonHelper.SaveFile(PermissionsManager.PermissionModule.permLista, Configuration.GetAbsoluteFilePath("data/permissions_list.json"));
            }
        }


    }
}

