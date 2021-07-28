using System;
using System.IO;
using System.Drawing;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Tasks;
using MCGalaxy.DB;

namespace NotAwesomeSurvival {

    public static class NasBlockChange {
        public static Scheduler breakScheduler;


        static Color[] blockColors = new Color[Block.MaxRaw + 1];
        const string terrainImageName = "terrain.png";
        
        public static bool Setup() {
            if (File.Exists("plugins/" + terrainImageName)) {
                File.Move("plugins/" + terrainImageName, Nas.Path + terrainImageName);
            }
            if (!File.Exists(Nas.Path + terrainImageName)) {
                Player.Console.Message("Could not locate {0} (needed for block particle colors)", terrainImageName);
                return false;
            }
            
            if (breakScheduler == null) breakScheduler = new Scheduler("BlockBreakScheduler");
            Bitmap terrain;
            terrain = new Bitmap(Nas.Path + terrainImageName);
            terrain = new Bitmap(terrain, terrain.Width / 16, terrain.Height / 16);
            //terrain.Save("plugins/nas/smolTerrain.png", System.Drawing.Imaging.ImageFormat.Png);

            for (BlockID blockID = 0; blockID <= Block.MaxRaw; blockID++) {
                BlockDefinition def = BlockDefinition.GlobalDefs[Block.FromRaw(blockID)];

                if (def == null && blockID < Block.CpeCount) { def = DefaultSet.MakeCustomBlock(Block.FromRaw(blockID)); }
                if (def == null) {
                    blockColors[blockID] = Color.White; continue;
                }
                int x = def.BackTex % 16;
                int y = def.BackTex / 16;
                blockColors[blockID] = terrain.GetPixel(x, y);

                //Logger.Log(LogType.Debug, "success color block "+blockID+".");
            }
            terrain.Dispose();
            return true;
        }

        const string ClickableBlocksKey = "__clickableBlocks_";
        const string LastClickedCoordsKey = ClickableBlocksKey + "lastClickedCoords";
        const string BreakAmountKey = ClickableBlocksKey + "breakAmount";
        const string BreakIDKey = ClickableBlocksKey + "breakID";
        const byte BreakEffectIDcount = 6;
        static byte BreakEffectID = 255;
        public const byte BreakMeterID = byte.MaxValue - BreakEffectIDcount;
        const int BreakMeterSpawnDelay = 100;

        static readonly object breakIDLocker = new object();

        static string LastClickedCoords(Player p) {
            return p.Extras.GetString(LastClickedCoordsKey, "-1 -1 -1");
        }
        static void SetLastClickedCoords(Player p, ushort x, ushort y, ushort z) {
            p.Extras[LastClickedCoordsKey] = x + " " + y + " " + z;
        }
        static int BreakAmount(Player p) {
            return p.Extras.GetInt(BreakAmountKey, 0);
        }
        static void SetBreakAmount(Player p, int amount) {
            p.Extras[BreakAmountKey] = amount;
        }

        static byte GetBreakID() {
            lock (breakIDLocker) {
                return BreakEffectID;
            }
        }
        static void SetBreakID(byte value) {
            lock (breakIDLocker) {
                if (value <= Byte.MaxValue - BreakEffectIDcount) { value = 255; }
                BreakEffectID = value;
            }
        }

        static void BreakBlock(NasPlayer np, ushort x, ushort y, ushort z, BlockID serverBlockID, NasBlock nasBlock) {
            if (np.nl == null) { return; }
            BlockID here = np.p.level.GetBlock(x, y, z);
            if (here != serverBlockID) { return; } //don't let them break it if the block changed since we've started
            
            
            //If there's a container and it's not empty or locked by someone else, it can't be broken
            //COPY PASTED IN 2 PLACES
            if (nasBlock.container != null &&
                np.nl.blockEntities.ContainsKey(x+" "+y+" "+z) &&
                (np.nl.blockEntities[x+" "+y+" "+z].drop != null || !np.nl.blockEntities[x+" "+y+" "+z].CanAccess(np.p) )
               )
            {
                return;
            }
            
            if (nasBlock.parentID != 0) {
                Drop drop = nasBlock.dropHandler(nasBlock.parentID);
                np.inventory.GetDrop(drop);

            } else {
                np.p.Message("You cannot collect {0} because it hasn't been defined as a survival block.",
                          Block.GetName(np.p, serverBlockID)
                         );
            }
            
            if (nasBlock.existAction != null) {
                nasBlock.existAction(np, nasBlock, false, x, y, z);
            }
            np.p.level.BlockDB.Cache.Add(np.p, x, y, z, BlockDBFlags.ManualPlace, here, Block.Air);
            np.nl.SetBlock(x, y, z, Block.Air);

            foreach (Player pl in np.p.level.players) {
                NassEffect.Define(pl, GetBreakID(), NassEffect.breakEffects[(int)nasBlock.material], blockColors[nasBlock.selfID]);
                NassEffect.Spawn(pl, GetBreakID(), NassEffect.breakEffects[(int)nasBlock.material], x, y, z, x, y, z);
            }
            SetBreakID((byte)(GetBreakID() - 1));
            np.justBrokeOrPlaced = true;
            
            if (!np.hasBeenSpawned) {
                np.p.Message("%chasBeenSpawned is %cfalse%S, this shouldn't happen if you didn't just die.");
                np.p.Message("%bPlease report to Goodly what you were doing before this happened");
            }
        }
        public static void CancelPlacedBlock(Player p, ushort x, ushort y, ushort z, ref bool cancel) {
            cancel = true;
            p.RevertBlock(x, y, z);
            Command.Find("tp").Use(p, "-precise ~ ~ ~");
        }
        public static void PlaceBlock(Player p, ushort x, ushort y, ushort z, BlockID serverBlockID, bool placing, ref bool cancel) {
            if (p.level.Config.Deletable && p.level.Config.Buildable) { return; }
            if (!placing) { p.Message("%cYou shouldn't be allowed to do this."); cancel = true; return; }

            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            BlockID clientBlockID = p.ConvertBlock(serverBlockID);
            NasBlock nasBlock = NasBlock.Get(clientBlockID);

            if (nasBlock.parentID == 0) {
                p.Message("You can't place undefined blocks.");
                CancelPlacedBlock(p, x, y, z, ref cancel);
                return;
            }

            int amount = np.inventory.GetAmount(nasBlock.parentID);
            if (amount < 1) {
                p.Message("%cYou don't have any {0}.", nasBlock.GetName(p));
                CancelPlacedBlock(p, x, y, z, ref cancel);
                return;
            }
            if (amount < nasBlock.resourceCost) {
                p.Message("%cYou need at least {0} {1} to place {2}.",
                          nasBlock.resourceCost, nasBlock.GetName(p), nasBlock.GetName(p, clientBlockID));
                CancelPlacedBlock(p, x, y, z, ref cancel);
                return;
            }
            np.inventory.SetAmount(nasBlock.parentID, -nasBlock.resourceCost);
            np.justBrokeOrPlaced = true;
        }
        
        public static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result) {
            if (p.level.Config.Deletable && p.level.Config.Buildable) { return; }
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            NasLevel nl = NasLevel.all[np.p.level.name];
            NasBlock nasBlock = NasBlock.blocksIndexedByServerBlockID[p.level.GetBlock(x, y, z)];
            if (nasBlock.existAction != null) {
                nasBlock.existAction(np, nasBlock, true, x, y, z);
            }
            if (nl != null) {
                nl.SimulateSetBlock(x, y, z);
            }
        }

        static void BreakTask(SchedulerTask task) {
            BreakInfo breakInfo = (BreakInfo)task.State;
            NasPlayer np = breakInfo.np;
            NasBlock nasBlock = breakInfo.nasBlock;
            bool coordsMatch =
                np.breakX == breakInfo.x &&
                np.breakY == breakInfo.y &&
                np.breakZ == breakInfo.z;

            if (coordsMatch &&
                np.breakAttempt == breakInfo.breakAttempt &&
                np.inventory.HeldItem == breakInfo.toolUsed
               ) {
                BreakBlock(np, breakInfo.x, breakInfo.y, breakInfo.z, breakInfo.serverBlockID, nasBlock);
                if (breakInfo.toolUsed.TakeDamage(nasBlock.damageDoneToTool)) {
                    np.inventory.BreakItem(ref breakInfo.toolUsed);
                }
                np.inventory.UpdateItemDisplay();

                np.lastLeftClickReleaseDate = DateTime.UtcNow;
                np.ResetBreaking();
                return;
            }
        }
        static void MeterTask(SchedulerTask task) {
            MeterInfo info = (MeterInfo)(task.State);
            Player p = info.p;
            int millisecs = info.milliseconds;
            millisecs -= BreakMeterSpawnDelay;
            NassEffect.Define(p, BreakMeterID, NassEffect.breakMeter, Color.White, (float)(millisecs / 1000.0f));
            NassEffect.Spawn(p, BreakMeterID, NassEffect.breakMeter, info.x, info.y, info.z, info.x, info.y, info.z);
        }

        private class BreakInfo {
            public NasPlayer np;
            public ushort x, y, z;
            public BlockID serverBlockID;
            public NasBlock nasBlock;
            public int breakAttempt;
            public Item toolUsed;
        }
        private class MeterInfo {
            public Player p;
            public int milliseconds;
            public float x, y, z;
        }

        public static void HandleLeftClick
        (Player p,
        MouseButton button, MouseAction action,
        ushort yaw, ushort pitch,
        byte entity, ushort x, ushort y, ushort z,
        TargetBlockFace face) {
            if (!p.agreed) { p.Message("You need to read and agree to the %b/rules%S to play"); return; }
            
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];

            if (action == MouseAction.Released) {
                np.ResetBreaking();
                np.lastAirClickDate = null;
                np.lastLeftClickReleaseDate = DateTime.UtcNow;
                NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
            }
            if (action == MouseAction.Pressed) {
                if (x == ushort.MaxValue ||
                    y == ushort.MaxValue ||
                    z == ushort.MaxValue) {
                    //p.Message("reset breaking since you clicked out of bounds");
                    np.ResetBreaking();
                    NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                    return;
                }

                BlockID serverBlockID = p.level.GetBlock(x, y, z);
                BlockID clientBlockID = p.ConvertBlock(serverBlockID);
                NasBlock nasBlock = NasBlock.Get(clientBlockID);
                if (nasBlock.durability == Int32.MaxValue) {
                    //p.Message("This block can't be broken");
                    np.ResetBreaking();
                    NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                    return;
                }
                
                //If there's a container and it's not empty or locked by someone else, it can't be broken
                //COPY PASTED IN 2 PLACES
                if (nasBlock.container != null &&
                    np.nl.blockEntities.ContainsKey(x+" "+y+" "+z) &&
                    (np.nl.blockEntities[x+" "+y+" "+z].drop != null || !np.nl.blockEntities[x+" "+y+" "+z].CanAccess(p) )
                   )
                {
                    np.ResetBreaking();
                    NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                    return;
                }

                Item heldItem = np.inventory.HeldItem;
                bool toolEffective = false;
                if (heldItem.prop.materialsEffectiveAgainst != null) {
                    foreach (NasBlock.Material mat in heldItem.prop.materialsEffectiveAgainst) {
                        //p.Message("heldItem is {0}, it is effective against {1} and this block is {2}",
                        //         heldItem.name, mat, nasBlock.material);
                        if (nasBlock.material == mat) {
                            toolEffective = true; break;
                        }
                    }
                }
                bool canBreakBlock = (heldItem.prop.tier >= nasBlock.tierOfToolNeededToBreak && toolEffective);
                if (nasBlock.tierOfToolNeededToBreak <= 0) { canBreakBlock = true; }
                if (!canBreakBlock) {
                    //p.Message("This block is too strong for your current tool.");
                    np.ResetBreaking();
                    NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                    return;
                }


                if (serverBlockID == Block.Air) {
                    if (np.lastAirClickDate == null) {
                        //p.Message("Whacked air");
                        np.lastAirClickDate = DateTime.UtcNow;
                    }

                    np.ResetBreaking();
                    NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                    return;
                }
                if (np.breakX == x && np.breakY == y && np.breakZ == z) {
                    //p.Message("Not starting another break on this block because you already are breaking it");
                    return;
                }

                NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);


                np.breakX = x;
                np.breakY = y;
                np.breakZ = z;

                np.breakAttempt++;


                //initial calculation
                const int swingTime = 260;
                int millisecs = (nasBlock.durability * swingTime) - swingTime;
                if (toolEffective) {
                    float multiplier = 1 - heldItem.prop.percentageOfTimeSaved;
                    millisecs = (int)((float)millisecs * multiplier);
                }
                if (millisecs < 0) { millisecs = 0; }
                //millisecs = 0;
                TimeSpan breakTime = TimeSpan.FromMilliseconds(np.holdingBreath ? millisecs*8 : millisecs);
                //initial calculation

                //lag compensation
                if (np.lastAirClickDate != null) {
                    //p.Message("subtracting LastAirBreakTime from breakTime");
                    TimeSpan sub = DateTime.UtcNow.Subtract((DateTime)np.lastAirClickDate);
                    breakTime -= sub;
                    millisecs = (int)breakTime.TotalMilliseconds;
                } else {
                    TimeSpan timeSinceLastBlockBroken = DateTime.UtcNow.Subtract(np.lastLeftClickReleaseDate);
                    int ping = p.Ping.AveragePing();
                    if (timeSinceLastBlockBroken.TotalMilliseconds >= ping) {
                        //p.Message("subtracting ping");
                        breakTime -= TimeSpan.FromMilliseconds(ping + (ping / 2));
                        millisecs = (int)breakTime.TotalMilliseconds;
                    }
                }
                np.lastAirClickDate = null;
                //lag compensation


                //Unk's Tunnel: 178.62.37.103:25570
                BreakInfo breakInfo = new BreakInfo();
                breakInfo.np = np;
                breakInfo.x = x;
                breakInfo.y = y;
                breakInfo.z = z;
                breakInfo.serverBlockID = serverBlockID;
                breakInfo.nasBlock = nasBlock;
                breakInfo.breakAttempt = np.breakAttempt;
                breakInfo.toolUsed = heldItem;

                SchedulerTask taskBreakBlock;
                taskBreakBlock = breakScheduler.QueueOnce(BreakTask, breakInfo, breakTime);



                MeterInfo meterInfo = new MeterInfo();
                meterInfo.p = p;
                meterInfo.milliseconds = millisecs;
                meterInfo.x = x;
                meterInfo.y = y;
                meterInfo.z = z;

                BlockDefinition def = BlockDefinition.GlobalDefs[Block.FromRaw(clientBlockID)];
                if (def == null && clientBlockID < Block.CpeCount) { def = DefaultSet.MakeCustomBlock(Block.FromRaw(clientBlockID)); }
                if (def != null) {
                    if (face == TargetBlockFace.AwayX) { DoOffset(def.MaxX, true, ref meterInfo.x); }
                    if (face == TargetBlockFace.TowardsX) { DoOffset(def.MinX, false, ref meterInfo.x); }

                    if (face == TargetBlockFace.AwayY) { DoOffset(def.MaxZ, true, ref meterInfo.y); }
                    if (face == TargetBlockFace.TowardsY) { DoOffset(def.MinZ, false, ref meterInfo.y); }
                    //blockdefinition's Y and Z bounds are swapped around
                    if (face == TargetBlockFace.AwayZ) { DoOffset(def.MaxY, true, ref meterInfo.z); }
                    if (face == TargetBlockFace.TowardsZ) { DoOffset(def.MinY, false, ref meterInfo.z); }
                }

                SchedulerTask taskDisplayMeter;
                taskDisplayMeter = breakScheduler.QueueOnce(MeterTask, meterInfo, TimeSpan.FromMilliseconds(BreakMeterSpawnDelay));
                p.Extras["nas_taskDisplayMeter"] = taskDisplayMeter;
            }
        }
        static void DoOffset(byte minOrMax, bool positive, ref float coord) {
            const float offset = 0.125f;

            if (positive) {
                coord -= 0.5f; //pull to minimum edge
                coord += (float)(minOrMax / 16.0f); //push to maximum edge
                coord += offset; //nudge out by offset
                return;
            }
            coord -= 0.5f; //pull to minimum edge
            coord += (float)(minOrMax / 16.0f); //push to minimum edge
            coord -= offset; //nudge out by offset
        }

    } //class NasBlockChange

}
