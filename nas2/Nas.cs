using System.IO;
using System.Threading;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Network;

//unknownshadow200: well player ids go from 0 up to 255. normal bots go from 127 down to 64, then 254 down to 127, then finally 63 down to 0.

//UnknownShadow200: FromRaw adds 256 if the block id is >= 66, and ToRaw subtracts 256 if the block id is >= 66
//"raw" is MCGalaxy's name for clientBlockID
///model |0.93023255813953488372093023255814

//gravestone drops upon death that contains your inventory
//different types of crafting stations
//furnace for smelting-style recipes



namespace NotAwesomeSurvival {

    public sealed class Nas : Plugin {
        public override string name { get { return "nas"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.5"; } }
        public override string creator { get { return "goodly"; } }
        const string textureURL = "https://dl.dropbox.com/s/xum0t6ld9g489ax/nas.zip";
        const string KeyPrefix = "nas_";
        public const string PlayerKey = KeyPrefix + "NasPlayer";
        public const string Path = "plugins/nas/";
        public const string SavePath = Path + "playerdata/";
        public static string GetSavePath(Player p) {
            return SavePath + p.name + ".json";
        }
        public static bool firstEverPluginLoad = true;
        public override void Load(bool startup) {
            if (Block.Props.Length != 1024) { //check for TEN_BIT_BLOCKS. Value is 512 on a default instance of MCGalaxy.
                Player.Console.Message("NAS: FAILED to load plugin. In order to run NAS, you must be using a version of MCGalaxy which allows 767 blocks.");
                Player.Console.Message("NAS: You can find instructions for 767 blocks here: https://github.com/UnknownShadow200/MCGalaxy/tree/master/Uploads (infid)");
                return;
            }
            
            if (File.Exists("plugins/Newtonsoft.Json")) {
                if (!File.Exists("Newtonsoft.Json.dll")) {
                    File.Move("plugins/Newtonsoft.Json", "Newtonsoft.Json.dll");
                }
                else {
                    File.Delete("plugins/Newtonsoft.Json");
                }
            }
            if (!File.Exists("Newtonsoft.Json.dll")) {
                Player.Console.Message("NAS: FAILED to load plugin. Could not find Newtonsoft.Json.dll"); return;
            }
            
            
            if (!Directory.Exists(Nas.Path)) { Directory.CreateDirectory(Nas.Path); }
            if (!Directory.Exists(NasLevel.Path)) { Directory.CreateDirectory(NasLevel.Path); }
            if (!Directory.Exists(Nas.SavePath)) { Directory.CreateDirectory(Nas.SavePath); }
            if (!Directory.Exists(NassEffect.Path)) { Directory.CreateDirectory(NassEffect.Path); }
            if (!Directory.Exists("blockprops")) { Directory.CreateDirectory("blockprops"); }
            
            //I HATE IT
            MoveFile("global.json", "blockdefs/global.json"); //blockdefs
            MoveFile("default.txt", "blockprops/default.txt"); //blockprops
            MoveFile("customcolors.txt", "text/customcolors.txt"); //custom chat colors
            MoveFile("command.properties", "properties/command.properties"); //command permissions
            MoveFile("ExtraCommandPermissions.properties", "properties/ExtraCommandPermissions.properties"); //extra command permissions
            MoveFile("ranks.properties", "properties/ranks.properties"); //ranks
            MoveFile("faq.txt", "text/faq.txt"); //faq
            MoveFile("messages.txt", "text/messages.txt"); //messages
            MoveFile("welcome.txt", "text/welcome.txt"); //welcome
            
            
            if (firstEverPluginLoad) {
                Server.Config.DefaultTexture = textureURL;
                Server.Config.DefaultColor = "&7";
                Server.Config.verifyadmins = false;
                Server.Config.WhitelistedOnly = true;
                Server.Config.EdgeLevel = 60;
                Server.Config.SidesOffset = -200;
                Server.Config.CloudsHeight = 200;
                Server.Config.MaxFogDistance = 512;
                Server.Config.SkyColor = "#1489FF";
                Server.Config.ShadowColor = "#888899";
                SrvProperties.Save();
            }
            //I HATE IT

            NasPlayer.Setup();
            NasBlock.Setup();
            if (!NassEffect.Setup()) { FailedLoad(); return; }
            if (!NasBlockChange.Setup()) { FailedLoad(); return; }
            ItemProp.Setup();
            Crafting.Setup();
            if (!DynamicColor.Setup()) { FailedLoad(); return; }
            Collision.Setup();

            OnPlayerConnectEvent.Register(OnPlayerConnect, Priority.High);
            OnPlayerClickEvent.Register(OnPlayerClick, Priority.High);
            OnBlockChangingEvent.Register(OnBlockChanging, Priority.High);
            OnBlockChangedEvent.Register(OnBlockChanged, Priority.High);
            OnPlayerMoveEvent.Register(OnPlayerMove, Priority.High);
            OnPlayerDisconnectEvent.Register(OnPlayerDisconnect, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.High);
            NasGen.Setup();
            NasLevel.Setup();
            
            
            if (Nas.firstEverPluginLoad) {
                //Player.Console.Message("GENERATING NEW MAP FIRST TIME EVER also main is {0}", lvl.name);
                int chunkOffsetX = 0, chunkOffsetZ = 0;
                string seed = "DEFAULT";
                if (!NasGen.GetSeedAndChunkOffset(Server.mainLevel.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ)) {
                    Player.Console.Message("NAS: main level is not a NAS level, generating a NAS level to replace it!");
                    seed = new Sharkbite.Irc.NameGenerator().MakeName().ToLower();
                    string mapName = seed+"_0,0";
                    Command.Find("newlvl").Use(Player.Console,
                                               mapName +
                                               " " + NasGen.mapWideness +
                                               " " + NasGen.mapTallness +
                                               " " + NasGen.mapWideness +
                                               " nasgen " + seed);
                    Server.Config.MainLevel = mapName;
                    SrvProperties.Save();
                    //Server.SetMainLevel(mapName);
                    Thread.Sleep(1000);
                    Server.Stop(true, "A server restart is required to initialize NAS plugin.");
                }
            }
        }
        static void MoveFile(string pluginFile, string destFile) {
            pluginFile = "plugins/"+pluginFile;
            if (File.Exists(pluginFile)) {
                if (File.Exists(destFile)) { File.Delete(destFile); }
                File.Move(pluginFile, destFile);
            }
            else {
                firstEverPluginLoad = false;
            }
        }
        static void FailedLoad() {
            Player.Console.Message("NAS: FAILED to load plugin. Please follow the instructions found on github.");
        }

        public override void Unload(bool shutdown) {
            NasPlayer.TakeDown();
            DynamicColor.TakeDown();
            OnPlayerConnectEvent.Unregister(OnPlayerConnect);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
            NasLevel.TakeDown();
        }

        static void OnPlayerConnect(Player p) {
            //Player.Console.Message("OnPlayerConnect");
            string path = GetSavePath(p);
            NasPlayer np;
            if (File.Exists(path)) {
                string jsonString = File.ReadAllText(path);
                np = JsonConvert.DeserializeObject<NasPlayer>(jsonString);
                np.SetPlayer(p);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Loaded save file " + path + "!");
            } else {
                np = new NasPlayer(p);
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Created new save file for " + p.name + "!");
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NasBlock.Default);

            //Q and E
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 16, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 18, 0, true));
            //arrow keys
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar up◙", 200, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar down◙", 208, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 203, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 205, 0, true));

            //WASD (lol)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen up◙", 17, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen down◙", 31, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen left◙", 30, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen right◙", 32, 0, true));

            //M and I
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar move◙", 50, 0, true)); //was 50 (M) was 42 (shift)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar inv◙", 19, 0, true)); //was 23 (i)

            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar delete◙", 45, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar confirmdelete◙", 25, 0, true));



        }
        static void OnPlayerCommand(Player p, string cmd, string message, CommandData data) {
            if (cmd.CaselessEq("setall")) {
                if (p.Rank < LevelPermission.Operator) { return; }
                
                foreach (Command _cmd in Command.allCmds) {
                    //p.Message("name {0}", _cmd.name);
                    Command.Find("cmdset").Use(p, _cmd.name + " Operator");
            
                }
                p.cancelcommand = true;
                return;
            }
            
            if (cmd.CaselessEq("goto") && p.Rank < LevelPermission.Operator && data.Context != CommandContext.SendCmd) {
                p.Message("You cannot use /goto manually. It is triggered automatically when you go to map borders.");
                p.cancelcommand = true;
                return;
            }
            
            if (cmd.CaselessEq("deleteall") && p.Rank >= LevelPermission.Operator) {
                if (message.Length == 0) { return; }
                string[] allMaps = LevelInfo.AllMapNames();
                Command deleteLvl = Command.Find("deletelvl");
                foreach (string mapName in allMaps) {
                    if (mapName.StartsWith(message)) {
                        deleteLvl.Use(p, mapName);
                    }
                }
                p.cancelcommand = true;
                return;
            }
            
            if (cmd.CaselessEq("color")) {
                if (message.Length == 0) { return; }
                string[] args = message.Split(' ');
                string color = args[args.Length-1];
                if (Matcher.FindColor(p, color) == "&h") {
                    p.Message("That color isn't allowed in names.");
                    p.cancelcommand = true; return;
                }
                return;
            }


            if (!cmd.CaselessEq("nas")) { return; }
            p.cancelcommand = true;
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            string[] words = message.Split(' ');

            if (words.Length > 1 && words[0] == "hotbar") {
                string hotbarFunc = words[1];
                if (words.Length > 2) {
                    string func2 = words[2];
                    if (hotbarFunc == "bagopen") {
                        if (!np.inventory.bagOpen) { return; }
                        if (func2 == "left") { np.inventory.MoveItemBarSelection(-1); return; }
                        if (func2 == "right") { np.inventory.MoveItemBarSelection(1); return; }
                        if (func2 == "up") { np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); return; }
                        if (func2 == "down") { np.inventory.MoveItemBarSelection(Inventory.itemBarLength); return; }
                    }
                    return;
                }

                if (hotbarFunc == "left") { np.inventory.MoveItemBarSelection(-1); return; }
                if (hotbarFunc == "right") { np.inventory.MoveItemBarSelection(1); return; }

                if (hotbarFunc == "up") { np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); return; }
                if (hotbarFunc == "down") { np.inventory.MoveItemBarSelection(Inventory.itemBarLength); return; }

                if (hotbarFunc == "move") { np.inventory.DoItemMove(); return; }
                if (hotbarFunc == "inv") { np.inventory.ToggleBagOpen(); return; }

                if (hotbarFunc == "delete") { np.inventory.DeleteItem(); return; }
                if (hotbarFunc == "confirmdelete") { np.inventory.DeleteItem(true); return; }


                return;
            }
        }
        static void OnPlayerDisconnect(Player p, string reason) {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            NasPlayer.SetLocation(np, p.level.name, p.Pos, p.Rot);
            //np.hasBeenSpawned = false;
            string jsonString;
            jsonString = JsonConvert.SerializeObject(np, Formatting.Indented);
            File.WriteAllText(GetSavePath(p), jsonString);
        }

        static void OnPlayerClick
        (Player p,
        MouseButton button, MouseAction action,
        ushort yaw, ushort pitch,
        byte entity, ushort x, ushort y, ushort z,
        TargetBlockFace face) {
            if (p.level.Config.Deletable && p.level.Config.Buildable) { return; }
            
            

            if (button == MouseButton.Left) { NasBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face); }
            
            
            
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            
            
            if (button == MouseButton.Middle && action == MouseAction.Pressed) {
                //np.ChangeHealth(0.5f);
                BlockID here = p.level.GetBlock(x, y, z);
                //p.Message("nasBlock {0}", NasBlock.blocksIndexedByServerBlockID[here].GetName(p));
                //NasBlock.blocksIndexedByServerBlockID[here].collideAction(np, NasBlock.blocks[1], true, x, y, z);
            }
            if (button == MouseButton.Right && action == MouseAction.Pressed) {
                //np.TakeDamage(0.5f);
            }
            
            
            
            
            if (!np.justBrokeOrPlaced) {
                np.HandleInteraction(button, action, x, y, z, entity, face);
            }
            
            if (action == MouseAction.Released) {
                np.justBrokeOrPlaced = false;
            }

        }

        static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel) {
            NasBlockChange.PlaceBlock(p, x, y, z, block, placing, ref cancel);
        }
        static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result) {
            NasBlockChange.OnBlockChanged(p, x, y, z, result);
        }
        static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch) {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            np.DoMovement(next, yaw, pitch);
        }

    }

}
