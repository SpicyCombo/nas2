using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Blocks;
using BlockID = System.UInt16;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public partial class NasPlayer {
        public static void Setup() {
            OnPlayerSpawningEvent.Register(OnPlayerSpawning, Priority.High);
        }
        public static void TakeDown() {
            OnPlayerSpawningEvent.Unregister(OnPlayerSpawning);
        }

        static void OnPlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning) {
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            np.nl = NasLevel.Get(p.level.name);
            np.SpawnPlayer(p.level, ref pos, ref yaw, ref pitch);
        }

        public void SpawnPlayer(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch) {
            if (level.Config.Deletable && level.Config.Buildable) { return; } //not a nas map
            canDoStuffBasedOnPosition = false;
            inventory.Setup();
            if (!hasBeenSpawned) { SpawnPlayerFirstTime(level, ref spawnPos, ref yaw, ref pitch); return; }

            if (transferInfo != null) {
                
                transferInfo.CalcNewPos();
                spawnPos = transferInfo.posBeforeMapChange;
                spawnPos.X = spawnPos.BlockX * 32 + 16;
                spawnPos.Z = spawnPos.BlockZ * 32 + 16;
                
                yaw = transferInfo.yawBeforeMapChange;
                pitch = transferInfo.pitchBeforeMapChange;
                
                atBorder = true;
                transferInfo = null;
            }
        }
        public void SpawnPlayerFirstTime(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch) {
            if (hasBeenSpawned) { return; }
            atBorder = true;
            if (!p.Model.EndsWith("|0.93023255813953488372093023255814")) { Command.Find("model").Use(p, "|0.93023255813953488372093023255814"); }

            spawnPos = new Position(location.X, location.Y, location.Z);
            yaw = this.yaw;
            pitch = this.pitch;
            Logger.Log(LogType.Debug, "Teleporting " + p.name + "!");

            if (level.name != levelName) {
                Player.Console.Message("{0}: trying to use /goto to move to the map they logged out in", p.name.ToUpper());
                //goto will call OnPlayerSpawning again to complete the spawn
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                p.HandleCommand("goto", levelName, data);
                return;
            }
            
            hasBeenSpawned = true;
            //p.Message("hasBeenSpawned set to {0}", hasBeenSpawned);
            Player.Console.Message("{0}: hasBeenSpawned set to {1}", p.name.ToUpper(), hasBeenSpawned);
            
        }
        
        
        [JsonIgnore] int round = 0;
        public void DoMovement(Position next, byte yaw, byte pitch) {
            UpdateHeldBlock();
            if (canDoStuffBasedOnPosition) { UpdateAir(); }
            CheckMapCrossing(p.Pos);
            //p.Message("%gPos {0} {1} {2} %b{3}", next.FeetBlockCoords.X, next.FeetBlockCoords.Y, next.FeetBlockCoords.Z, Environment.TickCount);
            if (canDoStuffBasedOnPosition) { DoNasBlockCollideActions(next); }
            CheckGround(p.Pos);
            UpdateCaveFog(next);
            round++;
        }
        [JsonIgnore] DateTime datePositionCheckingIsAllowed = DateTime.MinValue;
        [JsonIgnore] bool canDoStuffBasedOnPosition {
            get {
                if (DateTime.UtcNow >= datePositionCheckingIsAllowed) {
                    //p.Message("canDoStuffBasedOnPosition true");
                    return true;
                }
                //p.Message("canDoStuffBasedOnPosition false");
                return false;
            }
            set {
                //if (p != null) { p.Message("canDoStuffBasedOnPosition: {0}", value); }
                if (!value) { datePositionCheckingIsAllowed = DateTime.UtcNow.AddMilliseconds(2000+p.Ping.HighestPing()); }
            }
        }
                
        void CheckGround(Position next) {
            Position below = next;
            below.Y-= 2;
            float fallDamageMultiplier = 1;
            if (Collision.TouchesGround(p.level, bounds, below, out fallDamageMultiplier)) {
                float fallHeight = lastGroundedLocation.Y - next.Y;
                if (!canDoStuffBasedOnPosition && fallHeight > 0 && !hasBeenSpawned) { p.Message("trying to take fall damage but cant"); }
                if (fallHeight > 0 && canDoStuffBasedOnPosition) {
                    fallHeight /= 32f;
                    fallHeight-= 4;
                    
                    if (fallHeight > 0) {
                        float damage = (int)fallHeight * 2;
                        damage /= 4;
                        //p.Message("damage is {0}", damage*fallDamageMultiplier);
                        TakeDamage(damage*fallDamageMultiplier, DamageSource.Falling);
                    }
                }
                lastGroundedLocation = new MCGalaxy.Maths.Vec3S32(next.X, next.Y, next.Z);
            }
        }
        [JsonIgnore] bool atBorder = true;
        void CheckMapCrossing(Position next) {
            if (next.BlockX <= 0) {
                TryGoMapAt(-1, 0);
                return;
            }
            if (next.BlockX >= p.level.Width - 1) {
                TryGoMapAt(1, 0);
                return;
            }

            if (next.BlockZ <= 0) {
                TryGoMapAt(0, -1);
                return;
            }
            if (next.BlockZ >= p.level.Length - 1) {
                TryGoMapAt(0, 1);
                return;
            }
            atBorder = false;
        }
        void TryGoMapAt(int dirX, int dirZ) {
            if (atBorder) {
                //p.Message("Can't do it because already at border");
                return;
            }
            atBorder = true;
            int chunkOffsetX = 0, chunkOffsetZ = 0;
            string seed = "DEFAULT";
            if (!NasGen.GetSeedAndChunkOffset(p.level.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ)) { return; }
            
            chunkOffsetX += dirX;
            chunkOffsetZ += dirZ;
            string mapName = seed+"_"+chunkOffsetX + "," + chunkOffsetZ;
            if (File.Exists("levels/" + mapName + ".lvl")) {
                transferInfo = new TransferInfo(p, dirX, dirZ);
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                p.HandleCommand("goto", mapName, data);
            } else {
                if (NasGen.currentlyGenerating) {
                    p.Message("%cA map is already generating!");
                    return;
                }
                GenInfo info = new GenInfo();
                info.p = p;
                info.mapName = mapName;
                info.seed = seed;
                SchedulerTask taskGenMap;
                taskGenMap = NasGen.genScheduler.QueueOnce(GenTask, info, TimeSpan.Zero);
            }
        }
        class GenInfo {
            public Player p;
            public string mapName;
            public string seed;
        }
        static void GenTask(SchedulerTask task) {
            GenInfo info = (GenInfo)task.State;
            info.p.Message("Seed is {0}", info.seed);
            Command.Find("newlvl").Use(info.p, info.mapName + " " + NasGen.mapWideness + " " + NasGen.mapTallness + " " + NasGen.mapWideness + " nasgen " + info.seed);
        }
        [JsonIgnore] public TransferInfo transferInfo = null;
        public class TransferInfo {
            public TransferInfo(Player p, int chunkOffsetX, int chunkOffsetZ) {
                posBeforeMapChange = p.Pos;
                yawBeforeMapChange = p.Rot.RotY;
                pitchBeforeMapChange = p.Rot.HeadX;
                this.chunkOffsetX = chunkOffsetX;
                this.chunkOffsetZ = chunkOffsetZ;
            }
            public void CalcNewPos() {
                //* 32 because its in player units
                int xOffset = chunkOffsetX * NasGen.mapWideness * 32;
                int zOffset = chunkOffsetZ * NasGen.mapWideness * 32;
                posBeforeMapChange.X -= xOffset;
                posBeforeMapChange.Z -= zOffset;
            }
            [JsonIgnore] public Position posBeforeMapChange;
            [JsonIgnore] public byte yawBeforeMapChange;
            [JsonIgnore] public byte pitchBeforeMapChange;
            [JsonIgnore] public int chunkOffsetX, chunkOffsetZ;
        }

        public void UpdateCaveFog(Position next) {
            if (!NasLevel.all.ContainsKey(p.level.name)) { return; }

            const float change = 0.03125f;//0.03125f;
            if (curRenderDistance > targetRenderDistance) {
                curRenderDistance *= 1 - change;
                if (curRenderDistance < targetRenderDistance) { curRenderDistance = targetRenderDistance; }
            } else if (curRenderDistance < targetRenderDistance) {
                curRenderDistance *= 1 + change;
                if (curRenderDistance > targetRenderDistance) { curRenderDistance = targetRenderDistance; }
            }
            curFogColor = ScaleColor(curFogColor, targetFogColor);

            p.Send(Packet.EnvMapProperty(EnvProp.MaxFog, (int)curRenderDistance));
            p.Send(Packet.EnvColor(2, curFogColor.R, curFogColor.G, curFogColor.B));

            NasLevel nl = NasLevel.all[p.level.name];
            int x = next.BlockX;
            int z = next.BlockZ;
            x = Utils.Clamp(x, 0, (ushort)(p.level.Width - 1));
            z = Utils.Clamp(z, 0, (ushort)(p.level.Length - 1));
            ushort height = nl.heightmap[x, z];

            if (next.BlockCoords == p.Pos.BlockCoords) { return; }

            if (height < NasGen.oceanHeight) { height = NasGen.oceanHeight; }


            int distanceBelow = height - next.BlockY;
            int expFog = 0;
            if (distanceBelow >= NasGen.diamondDepth) {
                targetRenderDistance = 16;
                targetFogColor = NasGen.diamondFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.goldDepth) {
                targetRenderDistance = 24;
                targetFogColor = NasGen.goldFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.ironDepth) {
                targetRenderDistance = 32;
                targetFogColor = NasGen.ironFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.coalDepth) {
                targetRenderDistance = 64;
                targetFogColor = NasGen.coalFogColor;
                expFog = 1;
            } else {
                targetRenderDistance = Server.Config.MaxFogDistance;
                targetFogColor = Color.White;
                expFog = 0;
            }
            p.Send(Packet.EnvMapProperty(EnvProp.ExpFog, expFog));
        }

        static Color ScaleColor(Color cur, Color goal) {
            byte R = ScaleChannel(cur.R, goal.R);
            byte G = ScaleChannel(cur.G, goal.G);
            byte B = ScaleChannel(cur.B, goal.B);
            return Color.FromArgb(R, G, B);
        }
        static byte ScaleChannel(byte curChannel, byte goalChannel) {
            if (curChannel > goalChannel) {
                curChannel--;
            } else if (curChannel < goalChannel) {
                curChannel++;
            }
            return curChannel;
        }
    }

}
