namespace PermissionsManager
{
    using System;
    using Fougerite;
    using System.Collections.Generic;
    using System.IO;
    public class PermissionModule : Fougerite.Module
    {
        public override string Author
        {
            get
            {
                return "N0V4_DR0N3";
            }
        }
        public override string Description
        {
            get
            {
                return "Permissions and Groups";
            }
        }
        public override Version Version
        {
            get
            {
                return (new Version("1.0.0"));
            }
        }
        public override string Name
        {
            get
            {
                return "PermissionManager";
            }
        }

        IniParser config;
        public static List<string> DefualtFolders = new List<string>() { "config", "data" };
        public static string ConfigssFolder;
        public override void Initialize()
        {
            TimerEdit.TimerEditStart.Init();
            ConfigssFolder = ModuleFolder;
            Configuration.CreateFolderCheck(DefualtFolders);

            Hooks.OnConsoleReceived += OnConsoleReceived;

            try
            {
                Start();
            }catch(Exception ex)
            {
                Logger.Log("PermissionsManager Error Create Config: " + ex.Message);
            }

            TimerEdit.TimerEvento.Repeat(320, 0, () =>
            {
                Configuration.salvarConfigs();
            });
        }
        public override void DeInitialize()
        {
            TimerEdit.TimerEditStart.DeInitialize();
            Hooks.OnConsoleReceived -= OnConsoleReceived;
        }

        public class Config
        {
            public string playerNotFound;
            public string helpCommandGrant;
            public string helpCommandRevoke;
            public string helpCommandGrantUser;
            public string CommandGrantPermi;
            public string failAddPermission;
            public string helpCommandGrantGroup;
            public string CommandGrantPermiGroup;
            public string failAddPermissionGroup;
            public string helpCommandRevokeUser;
            public string helpCommandRevokeGroup;
            public string CommandRevokePermi;
            public string CommandRevokePermiGroup;
            public string failRemPermission;
            public string failRemPermissionGroup;
            public string helpCommandGroup;
            public string helpCommandGroupAdd;
            public string helpCommandGroupRem;
            public string CommandGroupAdd;
            public string CommandGroupRem;
            public string GroupNotFound;
            public string GroupExited;

            public Config Default()
            {
                playerNotFound = "Player not found!";
                helpCommandGrant = "Use pex.grant <user | group> <playerName | group> <permission> - to add a player permission | group!";
                helpCommandRevoke = "Use pex.revoke <user | group> <playerName | group> <permission> - to remove a player permission | group!";
                helpCommandGrantUser = "Use pex.grant user <playerName> <permission> - to give permission to the player!";
                CommandGrantPermi = "Permission {0} sitting for player {1}";
                failAddPermission = "Could not add permission {0} to {1}";
                helpCommandGrantGroup = "Use pex.grant group <groupName> <permission> - to add a group permission! ";
                CommandGrantPermiGroup = "You have added the permission {0} in group {1}";
                failAddPermissionGroup = "Could not add permission {0} to group {1}";
                helpCommandRevokeUser = "Use pex.revoke user <playername> <permission> - to remove a player permission";
                helpCommandRevokeGroup = "Use pex.revoke group <group> <permission> - to remove a group permission";
                CommandRevokePermi = "You have removed the permission {0} from the player {1}";
                CommandRevokePermiGroup = "You have removed the permission {0} from group {1}";
                failRemPermission = "Could not remove permission {0} from player {1}";
                failRemPermissionGroup = "Could not remove permission {0} from group {1}";
                helpCommandGroup = "Use pex.group <add | remove> <groupName> - to add | remove a group!";
                helpCommandGroupAdd = "Use pex.group add <groupName> - to create a group!";
                helpCommandGroupRem = "Use pex.group rem <groupName> - to delete a group!";
                CommandGroupAdd = "You have created the group {group}!";
                CommandGroupRem = "You have deleted {group}!";
                GroupNotFound = "Group not found!";
                GroupExited = "This group already exists!";
                return this;
            }
        }

        public static Config cfg = new Config();

        public static void Start()
        {
            cfg = Configuration.ReadyConfigChecked<Config>(cfg.Default(), "config/messages.json");
        }

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
                                if (!arg.HasArgs(1)) { Logger.Log(cfg.helpCommandGrant); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandGrantUser); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { Logger.Log(cfg.playerNotFound); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(cfg.helpCommandGrantUser); return; }
                                        if (addPermission(target.UID, arg.Args[2])) { Logger.Log(string.Format(cfg.CommandGrantPermi, arg.Args[2], target.Name)); return; }
                                        else Logger.Log(string.Format(cfg.failAddPermission, arg.Args[2], target.Name));
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandGrantGroup); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(cfg.helpCommandGrantGroup); return; }
                                        if (addPermission(0, arg.Args[2], arg.Args[1])) { Logger.Log(string.Format(cfg.CommandGrantPermiGroup, arg.Args[2], arg.Args[1])); return; }
                                        else Logger.Log(string.Format(cfg.failAddPermissionGroup, arg.Args[2], arg.Args[1]));
                                        break;
                                    default:
                                        Logger.Log(cfg.helpCommandGrant);
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(cfg.helpCommandRevoke); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandGrantUser); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { player.SendConsoleMessage(cfg.playerNotFound); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(cfg.helpCommandGrantUser); return; }
                                        if (addPermission(target.UID, arg.Args[2])) { player.SendConsoleMessage(string.Format(cfg.CommandGrantPermi, arg.Args[2], target.Name)); return; }
                                        else player.SendConsoleMessage(string.Format(cfg.failAddPermission, arg.Args[2], target.Name));
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandGrantGroup); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(cfg.helpCommandGrantGroup); return; }
                                        if (addPermission(0, arg.Args[2], arg.Args[1])) { player.SendConsoleMessage(string.Format(cfg.CommandGrantPermiGroup, arg.Args[2], arg.Args[1])); return; }
                                        else player.SendConsoleMessage(string.Format(cfg.failAddPermissionGroup, arg.Args[2], arg.Args[1]));
                                        break;
                                    default:
                                        player.SendConsoleMessage(cfg.helpCommandRevoke);
                                        break;
                                }
                            }
                            break;
                        case "revoke":
                            if (external)
                            {
                                if (!arg.HasArgs(1)) { Logger.Log(cfg.helpCommandRevoke); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandRevokeUser); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { Logger.Log(cfg.playerNotFound); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(cfg.helpCommandRevokeUser); return; }
                                        if (remPermission(target.UID, arg.Args[2], null)) { Logger.Log(string.Format(cfg.CommandRevokePermi, arg.Args[2], target.Name)); return; }
                                        else Logger.Log(string.Format(cfg.failRemPermission, arg.Args[2], target.Name));
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandRevokeGroup); return; }
                                        if (arg.Args.Length < 3) { Logger.Log(cfg.helpCommandRevokeGroup); return; }
                                        if (remPermission(0, arg.Args[1], arg.Args[2])) { Logger.Log(string.Format(cfg.CommandRevokePermiGroup, arg.Args[2], arg.Args[1])); return; }
                                        else Logger.Log(string.Format(cfg.failRemPermissionGroup, arg.Args[2], arg.Args[1]));
                                        break;
                                    default:
                                        Logger.Log(cfg.helpCommandRevoke);
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(cfg.helpCommandGrant); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "user":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandRevokeUser); return; }
                                        Player target = Server.GetServer().FindPlayer(arg.Args[1]);
                                        if (target == null) { player.SendConsoleMessage(cfg.playerNotFound); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(cfg.helpCommandRevokeUser); return; }
                                        if (remPermission(target.UID, arg.Args[1], null)) { player.SendConsoleMessage(string.Format(cfg.CommandGrantPermi, arg.Args[2], target.Name)); return; }
                                        else player.SendConsoleMessage(string.Format(cfg.failRemPermission, arg.Args[2], target.Name));
                                        break;
                                    case "group":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandRevokeGroup); return; }
                                        if (arg.Args.Length < 3) { player.SendConsoleMessage(cfg.helpCommandRevokeGroup); return; }
                                        if (remPermission(0, arg.Args[1], arg.Args[2])) { player.SendConsoleMessage(string.Format(cfg.CommandGrantPermiGroup, arg.Args[2], arg.Args[1])); return; }
                                        else player.SendConsoleMessage(string.Format(cfg.failRemPermissionGroup, arg.Args[2], arg.Args[1]));
                                        break;
                                    default:
                                        player.SendConsoleMessage(cfg.helpCommandGrant);
                                        break;
                                }
                            }
                            break;
                        case "group":
                            if (external)
                            {
                                if (!arg.HasArgs(1)) { Logger.Log(cfg.helpCommandGroup); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "add":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandGroupAdd); return; }
                                        if (groupLista.ContainsKey(arg.Args[1])) { Logger.Log(cfg.GroupExited); return; }
                                        var data = getGroup(arg.Args[1]);
                                        Logger.Log(cfg.CommandGroupAdd.Replace("{group}", arg.Args[1]));
                                        break;
                                    case "rem":
                                        if (!arg.HasArgs(2)) { Logger.Log(cfg.helpCommandGroupRem); return; }
                                        if (!groupLista.ContainsKey(arg.Args[1])) { Logger.Log(cfg.GroupNotFound); return; }
                                        groupLista.Remove(arg.Args[1]);
                                        Logger.Log(cfg.CommandGroupRem.Replace("{group}", arg.Args[1]));
                                        break;
                                    default:
                                        Logger.Log(cfg.helpCommandGroup);
                                        break;
                                }
                            }
                            else
                            {
                                Player player = Server.Cache[arg.argUser.userID];
                                if (!arg.HasArgs(1)) { player.SendConsoleMessage(cfg.helpCommandGroup); return; }
                                switch (arg.Args[0].ToLower())
                                {
                                    case "add":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandGroupAdd); return; }
                                        if (groupLista.ContainsKey(arg.Args[1])) { player.SendConsoleMessage(cfg.GroupExited); return; }
                                        var data = getGroup(arg.Args[1]);
                                        player.SendConsoleMessage(cfg.CommandGroupAdd.Replace("{group}", arg.Args[1]));
                                        break;
                                    case "rem":
                                        if (!arg.HasArgs(2)) { player.SendConsoleMessage(cfg.helpCommandGroupRem); return; }
                                        if (!groupLista.ContainsKey(arg.Args[1])) { player.SendConsoleMessage(cfg.GroupNotFound); return; }
                                        groupLista.Remove(arg.Args[1]);
                                        player.SendConsoleMessage(cfg.CommandGroupRem.Replace("{group}", arg.Args[1]));
                                        break;
                                    default:
                                        player.SendConsoleMessage(cfg.helpCommandGroup);
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
        public static Dictionary<ulong, List<string>> permLista = new Dictionary<ulong, List<string>>();
        public static Dictionary<string, Groups> groupLista = new Dictionary<string, Groups>();
        public Groups gp;
        public class Groups
        {
            public List<string> permissions { get; set; } = new List<string>();
            public List<ulong> users { get; set; } = new List<ulong>();
        }
        public Groups getGroup(string groupname)
        {
            if (!groupLista.TryGetValue(groupname, out gp))
            {
                gp = new Groups();
                groupLista.Add(groupname, gp);
            }
            return gp;
        }

        public bool hasPermission(ulong steamid, string permission)
        {
            foreach (KeyValuePair<string, Groups> pair in groupLista)
            {
                if (pair.Value.permissions.Contains(permission)) return true;
            }

            if (permLista.ContainsKey(steamid))
            {
                if (permLista[steamid].Contains(permission)) return true;
                return false;
            }
            return false;
        }
        public bool addPermission(ulong steamid, string permission, string group = null)
        {
            if (group != null)
            {
                if (groupLista.ContainsKey(group))
                {
                    var grupo = getGroup(group);
                    if (!grupo.permissions.Contains(permission))
                    {
                        grupo.permissions.Add(permission);
                        Configuration.salvarConfigs();
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (permLista.ContainsKey(steamid))
                {
                    if (!permLista[steamid].Contains(permission))
                    {
                        permLista[steamid].Add(permission);
                        Configuration.salvarConfigs();
                        return true;
                    }
                    return false;
                }
                else
                {
                    permLista.Add(steamid, new List<string>());
                    permLista[steamid].Add(permission);
                    Configuration.salvarConfigs();
                    return true;
                }
            }
            return false;
        }
        public bool remPermission(ulong steamid, string permission, string group = null)
        {
            if (group != null)
            {
                if (groupLista.ContainsKey(group))
                {
                    var grupo = getGroup(group);
                    if (grupo.permissions.Contains(permission))
                    {
                        grupo.permissions.Remove(permission);
                        Configuration.salvarConfigs();
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (permLista.ContainsKey(steamid))
                {
                    if (permLista[steamid].Contains(permission))
                    {
                        permLista[steamid].Remove(permission);
                        Configuration.salvarConfigs();
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

    }
}
