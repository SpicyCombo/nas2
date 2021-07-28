using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Network;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public partial class NasPlayer : NasEntity {
        [JsonIgnore] public Player p;
        [JsonIgnore] public NasBlock heldNasBlock = NasBlock.Default;
        [JsonIgnore] public ushort breakX = ushort.MaxValue, breakY = ushort.MaxValue, breakZ = ushort.MaxValue;
        [JsonIgnore] public int breakAttempt = 0;
        [JsonIgnore] public DateTime? lastAirClickDate = null;
        [JsonIgnore] public DateTime lastLeftClickReleaseDate = DateTime.MinValue;
        [JsonIgnore] public bool justBrokeOrPlaced = false;
        [JsonIgnore] public byte craftingAreaID = 0;
        [JsonIgnore] public bool isChewing = false;
        
        
        public void ResetBreaking() {
            breakX = breakY = breakZ = ushort.MaxValue;
            //NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
            if (p.Extras.Contains("nas_taskDisplayMeter")) {
                NasBlockChange.breakScheduler.Cancel((SchedulerTask)p.Extras["nas_taskDisplayMeter"]);
            }
        }
        public static NasPlayer GetNasPlayer(Player p) {
            if (!p.Extras.Contains(Nas.PlayerKey)) { return null; }
            return (NasPlayer)p.Extras[Nas.PlayerKey];
        }

        public Inventory inventory;
        [JsonIgnore] public bool hasBeenSpawned = false;

        [JsonIgnore] public Color targetFogColor = Color.White;
        [JsonIgnore] public Color curFogColor = Color.White;
        [JsonIgnore] public float targetRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnore] public float curRenderDistance = Server.Config.MaxFogDistance;
        
        public NasPlayer(Player p) {
            this.p = p;
            HP = 10;
            Air = 10;
            inventory = new Inventory(p);
            //hasBeenSpawned = false;
        }
        public void SetPlayer(Player p) {
            Player.Console.Message("setting player in inventory");
            this.p = p;
            inventory.SetPlayer(p);
        }
        public void HandleInteraction(MouseButton button, MouseAction action, ushort x, ushort y, ushort z, byte entityID, TargetBlockFace face) {
            if (button == MouseButton.Right && p.ClientHeldBlock != 0) {
                ushort xPlacing = x; ushort yPlacing = y; ushort zPlacing = z;
    			if (face == TargetBlockFace.AwayX)    { xPlacing++; }
    			if (face == TargetBlockFace.TowardsX) { xPlacing--; }
    			if (face == TargetBlockFace.AwayY)    { yPlacing++; }
    			if (face == TargetBlockFace.TowardsY) { yPlacing--; }
    			if (face == TargetBlockFace.AwayZ)    { zPlacing++; }
    			if (face == TargetBlockFace.TowardsZ) { zPlacing--; }
    			if (p.level.GetBlock(xPlacing, yPlacing, zPlacing) == Block.Air) {
    			    //p.Message("It's air");
    			    AABB worldAABB = bounds.OffsetPosition(p.Pos);
    			    //p.Message("worldAABB is {0}", worldAABB);
    			    //checking as if its a fully sized block
    			    AABB blockAABB = new AABB(0, 0, 0, 32, 32, 32);
    			    blockAABB = blockAABB.Offset(xPlacing * 32, yPlacing * 32, zPlacing * 32);
    			    //p.Message("blockAABB is {0}", blockAABB);
    			    
    			    if (!AABB.Intersects(ref worldAABB, ref blockAABB)) {
    			        //p.Message("it dont intersects");
    			        return;
    			    }
    			    //p.Message("it intersects");
    			}
            }
            BlockID serverBlockID = p.level.GetBlock(x, y, z);
            BlockID clientBlockID = p.ConvertBlock(serverBlockID);
            NasBlock nasBlock = NasBlock.Get(clientBlockID);
            if (nasBlock.interaction != null) {
                if (!canDoStuffBasedOnPosition) {
                    if (action == MouseAction.Released) { p.Message("%cPlease wait a moment before interacting with blocks"); }
                    return;
                }
                nasBlock.interaction(this, button, action, nasBlock, x, y, z);
            }
        }
        public override void ChangeHealth(float diff) {
            base.ChangeHealth(diff);
            DisplayHealth();
        }
        
        public override bool CanTakeDamage(DamageSource source) {
            //return false;
            if (p.invincible) { return false; }
            if (!hasBeenSpawned) { p.Message("this is a bug, please quit and rejoin to fix(?)"); return false; }
            
            if (source == DamageSource.Suffocating) {
                TimeSpan timeSinceSuffocation = DateTime.UtcNow.Subtract(lastSuffocationDate);
                if (timeSinceSuffocation.TotalMilliseconds < SuffocationMilliseconds) {
                    return false;
                }
                lastSuffocationDate = DateTime.UtcNow;
            }
            return true;
        }
        /// <summary>
        /// returns true if dead
        /// </summary>
        public override bool TakeDamage(float damage, DamageSource source, string customDeathReason = "") {
            if (!CanTakeDamage(source)) { return false; }
            
            if (damage == 0) { return false; }
            ChangeHealth(-damage);
            DisplayHealth("f", "&7[", "&7]");
            if (HP == 0) {
                if (customDeathReason.Length == 0) {
                    customDeathReason = NasEntity.DeathReason(source);
                }
                Die(customDeathReason);
                return true;
            }
            //p.Send(Packet.VelocityControl(0, 0.25f, 0, 0, 0, 0));
            SchedulerTask taskDisplayRed;
            taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
            
            return false;
        }
        static void FinishTakeDamage(SchedulerTask task) {
            NasPlayer np = (NasPlayer)task.State;
            np.DisplayHealth();
        }
        public override void Die(string reason) {
            hasBeenSpawned = false;
            //p.Message("hasBeenSpawned set to {0}", hasBeenSpawned);
            Player.Console.Message("{0}: hasBeenSpawned set to {1}", p.name.ToUpper(), hasBeenSpawned);
            TryDropGravestone();
            
            Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
            NasEntity.SetLocation(this, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
            lastGroundedLocation = new MCGalaxy.Maths.Vec3S32(Server.mainLevel.SpawnPos.X, Server.mainLevel.SpawnPos.Y, Server.mainLevel.SpawnPos.Z);
            reason = reason.Replace("@p", p.ColoredName);
            p.HandleDeath(Block.Stone, reason, false, true);
            p.SendCpeMessage(CpeMessageType.Announcement, "%cY O U  D I E D");
            curFogColor = Color.Black;
            curRenderDistance = 1;
            HP = maxHP;
            Air = maxAir;
            holdingBreath = false;
            
            inventory = new Inventory(p);
            inventory.Setup();
            inventory.DisplayHeldBlock(NasBlock.Default, 0);
            DisplayHealth();
        }
        void TryDropGravestone() {
            lock (NasBlock.Container.locker) {
                //if (inventory.GetAmount(1) == 0) {
                //    p.Message("You need to have at least one stone to drop a gravestone upon death.");
                //    return;
                //}
                //inventory.SetAmount(1, -1, true, true);
                Drop deathDrop = new Drop(inventory);
                if (deathDrop.blockStacks == null && deathDrop.items == null) {
                    //p.Message("You didn't drop a gravestone because you had no worldly possessions when you died.");
                    return;
                }
                
                Vec3S32 gravePos = p.Pos.FeetBlockCoords;
                p.level.ClampPos(gravePos);
                int x = gravePos.X;
                int y = gravePos.Y;
                int z = gravePos.Z;
                
                while (!CanPlaceGraveStone(x, y, z) ) {
                    y++;
                    if (y >= p.level.Height-1) {
                        p.Message("So you died in a column of blocks that literally reaches the sky, nice job!");
                        p.Message("No gravestone for you. Your stuff is lost forever.");
                        return;
                    }
                }
                
                
                //place tombstone
                nl.SetBlock(x, y, z, Block.FromRaw(647));
                NasBlock.Entity blockEntity = new NasBlock.Entity();
                blockEntity.drop = deathDrop;
                nl.blockEntities.Add(x+" "+y+" "+z, blockEntity);
                p.Message("You dropped a gravestone at {0} {1} {2} in {3}", x, y, z, p.level.name);
            }
        }
        bool CanPlaceGraveStone(int x, int y, int z) {
            BlockID here = nl.GetBlock(x, y, z);
            return NasBlock.CanPhysicsKillThis(here) || NasBlock.IsThisLiquid(here);
        }
        
        [JsonIgnore] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7]¼") {
            p.SendCpeMessage(whereHealthIsDisplayed, OxygenString()+" "+ prefix + HealthString(healthColor) + suffix);
        }
        private string HealthString(string healthColor) {
            StringBuilder builder = new StringBuilder("&8", (int)maxHP + 6);
            string final;
            float totalLostHealth = maxHP - HP;

            float lostHealthRemaining = totalLostHealth;
            for (int i = 0; i < totalLostHealth; ++i) {
                if (lostHealthRemaining < 1) {
                    builder.Append("&" + healthColor + "╝"); //broken heart
                } else {
                    builder.Append("♥"); //empty
                }
                lostHealthRemaining--;
            }

            builder.Append("&" + healthColor);
            for (int i = 0; i < (int)HP; ++i) { builder.Append("♥"); }

            final = builder.ToString();
            return final;
        }
        public override void UpdateAir() {
            base.UpdateAir();
            if (Air != AirPrev && Air == Math.Floor(Air)) {
                //p.Message("displaying health");
                DisplayHealth();
            }
        }
        private string OxygenString() {
            if (Air == maxAir) { return ""; }
            if (Air == 0) { return "&r┘"; }
            
            StringBuilder builder = new StringBuilder("", (int)maxAir + 6);
            string final;

            for (int i = 0; i < Air; ++i) {
                builder.Append('°');
            }

            final = builder.ToString();
            return final;
        }

        public void UpdateHeldBlock() {
            //p.ClientHeldBlock is server block ID
            BlockID clientBlockID = p.ConvertBlock(p.ClientHeldBlock);
            NasBlock nasBlock = NasBlock.Get(clientBlockID);

            if (nasBlock.parentID != heldNasBlock.parentID) {
                inventory.DisplayHeldBlock(nasBlock);
            }

            heldNasBlock = nasBlock;
        }
    }

}
