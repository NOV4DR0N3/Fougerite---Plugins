using System;
using System.Collections.Generic;
namespace PermissionsManager
{
    using Fougerite;
    using TimerEdit;
    public class PermissionModule : Fougerite.Module
    {

        public static System.Random random = new System.Random();

        public override string Author => "N0V4_DR0N3";
        public override string Description => "Permissions & Groups";
        public override Version Version => new Version("1.0.3");
        public override string Name => "PermissionsManager";

        public static List<string> folders = new List<string>() { "config", "data" };
        public static string folderPlugin;

        public override void Initialize()
        {
            TimerEditStart.Init();
            folderPlugin = ModuleFolder;

            Configuration.CreateFolderCheck(folders);
            Configuration.carregarConfigs();

            Hooks.OnConsoleReceived += OnConsoleReceived;
            Hooks.OnServerSaved += OnServerSaved;
            Hooks.OnServerShutdown += OnServerShutdown;
            Hooks.OnServerInit += OnServerInit;
            Hooks.OnPlayerConnected += OnPlayerConnected;

            // config / messages
            try
            {
                StartConfig();
                StartMessages();
            }catch(Exception ex)
            {
                Logger.Log("PermissionsManager: " + ex.Message);
            }
        }

        public override void DeInitialize()
        {
            TimerEditStart.DeInitialize();
            Configuration.salvarConfigs();

            Hooks.OnConsoleReceived -= OnConsoleReceived;
            Hooks.OnServerSaved -= OnServerSaved;
            Hooks.OnServerShutdown -= OnServerShutdown;
            Hooks.OnServerInit -= OnServerInit;
            Hooks.OnPlayerConnected -= OnPlayerConnected;
        }

        // TEMP PERMISSIONS
      
        public static Dictionary<string, tempPermissions> tempKeys = new Dictionary<string, tempPermissions>();
        public static tempPermissions tempp;
        public class tempPermissions
        {
            public string permission { get; set; }
            public string group { get; set; }
            public int days { get; set; }
        }
        public static tempPermissions getTempPermission(string value)
        {
            if(!tempKeys.TryGetValue(value, out tempp))
            {
                tempp = new tempPermissions();
                tempKeys.Add(value, tempp);
            }
            return tempp;
        }

        // 99% dessa merda não funcionar, 1% de funcionar mas com bug

        public static Dictionary<string, object> keys = new Dictionary<string, object>();
        public static Dictionary<string, object> keysActived = new Dictionary<string, object>();
        public static string GerarKey()
        {
            string key = string.Empty;
            for (int i = 0; i < cfg.keySize; i++)
            {
                //   Random random = new Random();
                int codigo = Convert.ToInt32(random.Next(48, 122).ToString());

                if ((codigo >= 48 && codigo <= 57) || (codigo >= 97 && codigo <= 122))
                {
                    string _char = ((char)codigo).ToString();
                    if (!key.Contains(_char))
                    {
                        key += _char;
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    i--;
                }
            }
            return key;
        }

        public static DateTime time = DateTime.Now;

        public static bool createKey(string value = null, int definition = 0, int days = 1)
        {
            string keyGerada = GerarKey().ToUpper();
            if (days <= 0)
                days = 1;
            if (value.Length == 0 || value == string.Empty)
                return false;
            if (definition > 2 || definition <= 0)
                return false;
            if (keys.ContainsKey(keyGerada))
                keyGerada = GerarKey().ToUpper();

            var data = new Dictionary<string, object>();
            data.Add("value", value);
            data.Add("definition", definition);
            data.Add("days", days);
            keys.Add(keyGerada, data);
            Server.GetServer().Broadcast(keyGerada);
            return true;         
        }

        public static bool activeKey(Fougerite.Player player, string key)
        {
            key = key.ToUpper();
            if (key.Length == 0 || key == string.Empty) return false;
            if (!keys.ContainsKey(key)) return false;

            var data = keys[key] as Dictionary<string, object>;
            var info = new Dictionary<string, object>();

            info.Add("player_name", player.Name);
            info.Add("player_steamid", player.SteamID);
            info.Add("player_ip", player.IP);
            info.Add("value", data["value"].ToString());
            info.Add("date_start", time);
            DateTime date_end = time.AddDays(Convert.ToDouble(data["days"]));
            info.Add("date_end", date_end);
            info.Add("ticks", date_end.Ticks);
            info.Add("definition", data["definition"]);
            keysActived.Add(key, info);
            keys.Remove(key);
            return true;
        }
        public static void verificarHard()
        {
            if (keysActived.Count == 0) return;
            foreach(var pair in keysActived)
            {
                var data = keysActived[pair.Key] as Dictionary<string, object>;
                if(Convert.ToUInt32(data["ticks"]) <= time.Ticks)
                {
                    if(Convert.ToInt32(data["definition"]) == 1)
                    {
                        revokePermission(Convert.ToUInt32(data["player_steamid"]), data["value"].ToString(), null);
                        keysActived.Remove(pair.Key);
                    }
                    else if (Convert.ToInt32(data["definition"]) == 2)
                    {
                        remPlayerGroup(Convert.ToUInt32(data["player_steamid"]), data["value"].ToString());
                        keysActived.Remove(pair.Key);
                    }
                }
            }
        }
        // Hooks Server And PlayerConnected

        private void OnServerSaved(int Amount, double Seconds)
        {
            if (cfg.SaveConfigTogetherToTheMap)
                Configuration.salvarConfigs();
        }
        private void OnServerShutdown()
        {
            Configuration.salvarConfigs();
        }
        private void OnServerInit()
        {
            if (cfg.SaveConfigPerSeconds)
            {
                TimerEvento.Repeat(cfg.SaveConfigSeconds, 0, () =>
                {
                    Configuration.salvarConfigs();
                });
            }
        }
        private void OnPlayerConnected(Player player)
        {
            if (!player_permissions.ContainsKey(player.UID))
            {
                var data = getPermission(player.UID);
                data.player_nick = player.Name;
                data.player_steamid = player.UID;
                Configuration.salvarConfigs();
            }
            else
            {
                if(player_permissions.ContainsKey(player.UID))
                {
                    var dataplayer = getPermission(player.UID);
                    if (dataplayer.player_nick != player.Name)
                        dataplayer.player_nick = player.Name;
                }
            }
        }

        // CONFIGURATION - EN

        public class Config
        {
            public string chatPrefix;

            public bool SaveConfigTogetherToTheMap; // false
            public bool SaveConfigPerSeconds; // true

            public float SaveConfigSeconds; // 360
            public int keySize;

            public Config Default()
            {
                chatPrefix = "PermissionsManager";

                SaveConfigTogetherToTheMap = true;
                SaveConfigPerSeconds = false;
                SaveConfigSeconds = 360f;
                keySize = 10;
                return this;
            }
        }
        public static Config cfg = new Config();
        public static void StartConfig()
        {
            cfg = Configuration.ReadyConfigChecked<Config>(cfg.Default(), "config/config.json");
        }

        // MESSAGES - LANG EN

        public class Messages
        {
            public Dictionary<string, string> messages;
            public Messages Default()
            {
                messages = new Dictionary<string, string>()
                {
                    { "groupAddPlayer", "You have successfully added {0} to {1} group!" },
                    { "groupRemPlayer", "You successfully removed {0} from group {1}!" },
                    { "helpCommandGroupUser", "Use pex.group user <group> <playerName> - to add it to the group" },
                    { "helpCommandGroupRUser", "Use pex.group ruser <group> <playerName> - to remove the player from the group." },
                    { "playerNotGroup", "Player is not in this group" },
                    { "playerGroup", "The player is already in this group" },
                    { "playerNotFound", "Player not found!" },
                    { "helpCommandGrant", "Use pex.grant <user | group> <playerName | group> <permission> - to add a player permission | group!" },
                    { "helpCommandRevoke", "Use pex.revoke <user | group> <playerName | group> <permission> - to remove a player permission | group!" },
                    { "helpCommandGrantUser", "Use pex.grant user <playerName> <permission> - to give permission to the player!" },
                    { "CommandGrantPermi", "Permission {0} sitting for player {1}" },
                    { "failAddPermission", "Could not add permission {0} to {1}" },
                    { "helpCommandGrantGroup", "Use pex.grant group <groupName> <permission> - to add a group permission! " },
                    { "CommandGrantPermiGroup", "You have added the permission {0} in group {1}" },
                    { "failAddPermissionGroup", "Could not add permission {0} to group {1}" },
                    { "helpCommandRevokeUser", "Use pex.revoke user <playername> <permission> - to remove a player permission" },
                    { "helpCommandRevokeGroup", "Use pex.revoke group <group> <permission> - to remove a group permission" },
                    { "CommandRevokePermi", "You have removed the permission {0} from the player {1}" },
                    { "CommandRevokePermiGroup", "You have removed the permission {0} from group {1}" },
                    { "failRemPermission", "Could not remove permission {0} from player {1}" },
                    { "failRemPermissionGroup", "Could not remove permission {0} from group {1}" },
                    { "helpCommandGroup", "Use pex.group <add | remove> <groupName> - to add | remove a group!" },
                    { "helpCommandGroupAdd", "Use pex.group add <groupName> - to create a group!" },
                    { "helpCommandGroupRem", "Use pex.group rem <groupName> - to delete a group!" },
                    { "CommandGroupAdd", "You have created the group {group}!" },
                    { "CommandGroupRem", "You have deleted {group}!" },
                    { "GroupNotFound", "Group not found!" },
                    { "GroupExited", "This group already exists!" },
                    { "", "" }
                };
                return this;
            }
        }
        public static Messages messages = new Messages();
        public static void StartMessages()
        {
            messages = Configuration.ReadyConfigChecked<Messages>(messages.Default(), "config/messages.json");
        }
        public static string getMessage(string message)
        {
            string retorno = string.Empty;
            if (messages.messages.ContainsKey(message))
                retorno = messages.messages[message];
            return retorno;
        }


        // CONSOLE COMMANDS

        private void OnConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            string function = arg.Function;
            string cmd = arg.Class;
            switch (cmd.ToLower())
            {
                case "pex":
                    switch (function.ToLower())
                    {
                        case "grant":
                            if (external)
                            {
                                if (!arg.HasArgs(1)) { Logger.Log(getMessage("helpCommandGrant")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGrantUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { Logger.Log(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandGrantUser")); return; }
                                        if (grantPermission(target.UID, arg.Args[2], null)) { Logger.Log(string.Format(getMessage("CommandGrantPermi"), arg.Args[2], target.Name)); return; }
                                        else Logger.Log(string.Format(getMessage("failAddPermission"), arg.Args[2], target.Name));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGrantGroup")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandGrantGroup")); return; }
                                        if (grantPermission(0, arg.Args[1], arg.Args[2])) { Logger.Log(string.Format(getMessage("CommandGrantPermiGroup"), arg.Args[2], arg.Args[1])); return; }
                                        else Logger.Log(string.Format(getMessage("failAddPermissionGroup"), arg.Args[2], arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "key":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("Use pex.grant key <permission> <days> - para criar um key com permissão!")); return; }

                                        if (arg.Args.Length < 4) { Logger.Log(getMessage("helpCommandGrantUser")); return; }

                                        string key = GerarKey();
                                        if(!tempKeys.ContainsKey(key.ToUpper()))
                                        {
                                            var data = getTempPermission(key.ToUpper());
                                            data.days = Convert.ToInt32(arg.Args[2]);
                                            data.permission = arg.Args[1];
                                            Logger.Log(string.Format("Sucesso ao gerar a key, -> ", key));
                                        }else
                                        {
                                            key = GerarKey();
                                            if (!tempKeys.ContainsKey(key.ToUpper()))
                                            {
                                                var data = getTempPermission(key.ToUpper());
                                                data.days = Convert.ToInt32(arg.Args[2]);
                                                data.permission = arg.Args[1];
                                                Logger.Log(string.Format("Sucesso ao gerar a key, -> ", key));
                                            }
                                        }
                                        break;
                                    default:
                                        Logger.Log(getMessage("helpCommandGrant"));
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(getMessage("helpCommandRevoke")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGrantUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { player.SendConsoleMessage(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandGrantUser")); return; }
                                        if (grantPermission(target.UID, arg.Args[2], null)) { player.SendConsoleMessage(string.Format(getMessage("CommandGrantPermi"), arg.Args[2], target.Name)); return; }
                                        else player.SendConsoleMessage(string.Format(getMessage("failAddPermission"), arg.Args[2], target.Name));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGrantGroup")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandGrantGroup")); return; }
                                        if (grantPermission(0, arg.Args[2], arg.Args[1])) { player.SendConsoleMessage(string.Format(getMessage("CommandGrantPermiGroup"), arg.Args[2], arg.Args[1])); return; }
                                        else player.SendConsoleMessage(string.Format(getMessage("failAddPermissionGroup"), arg.Args[2], arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    default:
                                        player.SendConsoleMessage(getMessage("helpCommandRevoke"));
                                        break;
                                }
                            }
                            break;
                        case "revoke":
                            if (external)
                            {
                                if (!arg.HasArgs(1)) { Logger.Log(getMessage("helpCommandRevoke")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandRevokeUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { Logger.Log(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandRevokeUser")); return; }
                                        if (revokePermission(target.UID, arg.Args[2], null)) { Logger.Log(string.Format(getMessage("CommandRevokePermi"), arg.Args[2], target.Name)); return; }
                                        else Logger.Log(string.Format(getMessage("failRemPermission"), arg.Args[2], target.Name));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandRevokeGroup")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandRevokeGroup")); return; }
                                        if (revokePermission(0, arg.Args[1], arg.Args[2])) { Logger.Log(string.Format(getMessage("CommandRevokePermiGroup"), arg.Args[2], arg.Args[1])); return; }
                                        else Logger.Log(string.Format(getMessage("failRemPermissionGroup"), arg.Args[2], arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    default:
                                        Logger.Log(getMessage("helpCommandRevoke"));
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(getMessage("helpCommandGrant")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandRevokeUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { player.SendConsoleMessage(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandRevokeUser")); return; }
                                        if (revokePermission(target.UID, arg.Args[1], null)) { player.SendConsoleMessage(string.Format(getMessage("CommandGrantPermi"), arg.Args[2], target.Name)); return; }
                                        else player.SendConsoleMessage(string.Format(getMessage("failRemPermission"), arg.Args[2], target.Name));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandRevokeGroup")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandRevokeGroup")); return; }
                                        if (revokePermission(0, arg.Args[1], arg.Args[2])) { player.SendConsoleMessage(string.Format(getMessage("CommandGrantPermiGroup"), arg.Args[2], arg.Args[1])); return; }
                                        else player.SendConsoleMessage(string.Format(getMessage("failRemPermissionGroup"), arg.Args[2], arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    default:
                                        player.SendConsoleMessage(getMessage("helpCommandGrant"));
                                        break;
                                }
                            }
                            break;
                        case "group":
                            if (external)
                            {
                                if (!arg.HasArgs(1)) { Logger.Log(getMessage("helpCommandGroup")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "add":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGroupAdd")); return; }
                                        if (groupLista.ContainsKey(arg.Args[1])) { Logger.Log(getMessage("GroupExited")); return; }
                                        var data = getGroup(arg.Args[1]);
                                        Logger.Log(getMessage("CommandGroupAdd").Replace("{group}", arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "rem":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGroupRem")); return; }
                                        if (!groupLista.ContainsKey(arg.Args[1])) { Logger.Log(getMessage("GroupNotFound")); return; }
                                        groupLista.Remove(arg.Args[1]);
                                        Logger.Log(getMessage("CommandGroupRem").Replace("{group}", arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "user":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGroupUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { Logger.Log(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandGroupUser")); return; }
                                        if (addPlayerGroup(target.UID, arg.Args[2])) { Logger.Log(string.Format(getMessage("groupAddPlayer"), target.Name, arg.Args[2])); return; }
                                        else Logger.Log(getMessage("playerGroup"));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "ruser":
                                        if (!arg.HasArgs(2)) { Logger.Log(getMessage("helpCommandGroupRUser")); return; }
                                        Player target2 = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target2 == null) { Logger.Log(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(getMessage("helpCommandGroupRUser")); return; }
                                        if (remPlayerGroup(target2.UID, arg.Args[2])) { Logger.Log(string.Format(getMessage("groupRemPlayer"), target2.Name, arg.Args[2])); return; }
                                        else Logger.Log(getMessage("playerNotGroup"));
                                        Configuration.salvarConfigs();
                                        break;
                                    default:
                                        Logger.Log(getMessage("helpCommandGroup"));
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(getMessage("helpCommandGroup")); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "add":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGroupAdd")); return; }
                                        if (groupLista.ContainsKey(arg.Args[1])) { player.SendConsoleMessage(getMessage("GroupExited")); return; }
                                        var data = getGroup(arg.Args[1]);
                                        player.SendConsoleMessage(getMessage("CommandGroupAdd").Replace("{group}", arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "rem":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGroupRem")); return; }
                                        if (!groupLista.ContainsKey(arg.Args[1])) { player.SendConsoleMessage(getMessage("GroupNotFound")); return; }
                                        groupLista.Remove(arg.Args[1]);
                                        player.SendConsoleMessage(getMessage("CommandGroupRem").Replace("{group}", arg.Args[1]));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "user":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGroupUser")); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { player.SendConsoleMessage(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandGroupUser")); return; }
                                        if (addPlayerGroup(target.UID, arg.Args[2])) { player.SendConsoleMessage(string.Format(getMessage("groupAddPlayer"), target.Name, arg.Args[2])); return; }
                                        else player.SendConsoleMessage(getMessage("playerGroup"));
                                        Configuration.salvarConfigs();
                                        break;
                                    case "ruser":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(getMessage("helpCommandGroupRUser")); return; }
                                        Player target2 = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target2 == null) { player.SendConsoleMessage(getMessage("playerNotFound")); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(getMessage("helpCommandGroupRUser")); return; }
                                        if (remPlayerGroup(target2.UID, arg.Args[2])) { player.SendConsoleMessage(string.Format(getMessage("groupRemPlayer"), target2.Name, arg.Args[2])); return; }
                                        else player.SendConsoleMessage(getMessage("playerNotGroup"));
                                        Configuration.salvarConfigs();
                                        break;
                                    default:
                                        player.SendConsoleMessage(getMessage("helpCommandGroup"));
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        // PERMISSIONS

        public static Dictionary<ulong, PlayersPermissions> player_permissions = new Dictionary<ulong, PlayersPermissions>();
        public static PlayersPermissions playerp;
        public class PlayersPermissions
        {
            public string player_nick { get; set; }
            public ulong player_steamid { get; set; }
            public List<string> permissions = new List<string>();
        }
        public static PlayersPermissions getPermission(ulong player)
        {
            if(!player_permissions.TryGetValue(player, out playerp))
            {
                playerp = new PlayersPermissions();
                player_permissions.Add(player, playerp);
            }
            return playerp;
        }

        public static bool hasPermission(ulong player, string permission)
        {
            if(player_permissions.ContainsKey(player))
            {
                var data = getPermission(player);
                if (data.permissions.Contains(permission))
                    return true;
            }
            else
            {
                foreach (KeyValuePair<string, Groups> pair in groupLista)
                {
                    if (pair.Value.permissions.Contains(permission))
                    {
                        if (pair.Value.users.Contains(player))
                            return true;
                    }
                }
            }
            return false;
        }
        public static bool grantPermission(ulong player, string value, string value2)
        {
            Fougerite.Player target = Server.GetServer().FindPlayer(player.ToString());
            if (target != null || value.Length == 17 || player != 0)
            {
                if (player_permissions.ContainsKey(player))
                {
                    var data = getPermission(player);
                    if (!data.permissions.Contains(value))
                    {
                        data.permissions.Add(value);
                        return true;
                    }
                    else
                        return true;
                }
            }
            else
            {
                if(groupLista.ContainsKey(value))
                {
                    var data_grupo = getGroup(value);
                    if (!data_grupo.permissions.Contains(value2))
                    {
                        data_grupo.permissions.Add(value2);
                        return true;
                    }
                    else
                        return true;
                }
            }
            return false;
        }
        public static bool revokePermission(ulong player, string value, string value2)
        {
            Fougerite.Player target = Server.GetServer().FindPlayer(player.ToString());
            if (target != null || value.Length == 17 || player != 0)
            {
                if (player_permissions.ContainsKey(player))
                {
                    var data = getPermission(player);
                    if (data.permissions.Contains(value))
                    {
                        data.permissions.Remove(value);
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            else
            {
                if(groupLista.ContainsKey(value))
                {
                    var grupo_data = getGroup(value);
                    if (!grupo_data.permissions.Contains(value2))
                    {
                        grupo_data.permissions.Add(value2);
                        return true;
                    }
                    else
                        return true;
                }
            }
            return false;
        }

        // GROUPS 

        public static Dictionary<string, Groups> groupLista = new Dictionary<string, Groups>();
        public static Groups gp;
        public class Groups
        {
            public List<string> permissions { get; set; } = new List<string>();
            public List<ulong> users { get; set; } = new List<ulong>();
        }
        public static Groups getGroup(string groupname)
        {
            if (!groupLista.TryGetValue(groupname, out gp))
            {
                gp = new Groups();
                groupLista.Add(groupname, gp);
            }
            return gp;
        }

        public static bool addPlayerGroup(ulong steamid, string groupName)
        {
            if(groupLista.ContainsKey(groupName))
            {
                var data = getGroup(groupName);
                if (data.users.Contains(steamid)) return false;
                data.users.Add(steamid);
                return true;
            }
            return false;
        }
        public static bool remPlayerGroup(ulong steamid, string groupName)
        {
            if(groupLista.ContainsKey(groupName))
            {
                var data = getGroup(groupName);
                if(data.users.Contains(steamid))
                {
                    data.users.Remove(steamid);
                    return true;
                }
                return false;
            }
            return false;
        }
        
    }
}
