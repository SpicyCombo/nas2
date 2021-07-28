using System;
using System.Threading;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;

using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {
        public static class Collision {
            
            public static void Setup() {
                SetupBlockBounds(Server.mainLevel);
            }
            
            public static void SetupBlockBounds(Level lvl) {
                NasBlock.blocksIndexedByServerBlockID = new NasBlock[Block.ExtendedCount];
                for (BlockID blockID = 0; blockID < Block.ExtendedCount; blockID++) {
                    NasBlock.blocksIndexedByServerBlockID[blockID] = GetNasBlockAndFillInCollisionInformation(blockID, lvl);
                }
                NasLevel.OnLevelLoaded(lvl);
            }
            public static NasBlock GetNasBlockAndFillInCollisionInformation (BlockID serverBlockID, Level lvl) {
                bool collides = true;
                AABB bounds;
                float fallDamageMultiplier = 1;
                NasBlockCollideAction collideAction = NasBlock.DefaultSolidCollideAction();
                BlockDefinition def = lvl.GetBlockDef(serverBlockID);
                if (def != null) {
                    bounds = new AABB(def.MinX * 2, def.MinZ * 2, def.MinY * 2,
                                    def.MaxX * 2, def.MaxZ * 2, def.MaxY * 2);
                    
                    switch (def.CollideType) {
                        case CollideType.ClimbRope:
                        case CollideType.LiquidWater:
                        case CollideType.SwimThrough:
                            bounds.Max.Y -= 4;
                            fallDamageMultiplier = 0;
                            collideAction = null; //DefaultLiquidCollideAction?
                            break;
                        case CollideType.WalkThrough:
                            collideAction = null;
                            collides = false;
                            break;
                        default:
                        	break;
                    }
                }
                else if (serverBlockID >= Block.Extended) {
                    bounds = new AABB(0, 0, 0, 32, 32, 32);
                }
                else {
                    BlockID core = Block.Convert(serverBlockID);
                    bounds = new AABB(0, 0, 0, 32, DefaultSet.Height(core) * 2, 32);
                }
                NasBlock nb = NasBlock.Get(ConvertToClientBlockID(serverBlockID, lvl));
                //physics blocks like cold_water are also attempted to setup in this list, so only use the properties from the first one
                if (nb.fallDamageMultiplier == -1) {
                    nb.collides = collides;
                    nb.bounds = bounds;
                    nb.fallDamageMultiplier = fallDamageMultiplier;
                    if (nb.collideAction == null) { nb.collideAction = collideAction; }
                }
                return nb;
            }
            
            public static BlockID ConvertToClientBlockID(BlockID serverBlockID, Level lvl) {
                BlockID clientBlockID;
                if (serverBlockID >= Block.Extended) {
                    clientBlockID = Block.ToRaw(serverBlockID);
                } else {
                    clientBlockID = Block.Convert(serverBlockID);
                    if (clientBlockID >= Block.CpeCount) clientBlockID = Block.Orange;
                }
                return clientBlockID;
            }
            
            public static bool TouchesGround(Level lvl, AABB entityAABB, Position entityPos, out float fallDamageMultiplier) {
                fallDamageMultiplier = 1;
                AABB worldAABB = entityAABB.OffsetPosition(entityPos);
                worldAABB.Min.X++;
                worldAABB.Min.Z++;
                entityPos.X += entityAABB.Max.X;
                entityPos.Z += entityAABB.Max.Z;
                if (_TouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier)) { return true; }
                entityPos.X += entityAABB.Min.X*2;
                if (_TouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier)) { return true; }
                entityPos.Z += entityAABB.Min.Z*2;
                if (_TouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier)) { return true; }
                entityPos.X += entityAABB.Max.X*2;
                if (_TouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier)) { return true; }
                return false;
            }
            public static bool _TouchesGround(Level lvl, AABB entityAABB, Position posToGetBlock, out float fallDamageMultiplier) {
                fallDamageMultiplier = 1;
                int x = posToGetBlock.FeetBlockCoords.X;
                int y = posToGetBlock.FeetBlockCoords.Y;
                int z = posToGetBlock.FeetBlockCoords.Z;
                BlockID serverBlockID = lvl.GetBlock((ushort)x,
                                                     (ushort)y,
                                                     (ushort)z);
                if (serverBlockID == Block.Air) { return false; }
                NasBlock nasBlock = NasBlock.blocksIndexedByServerBlockID[serverBlockID];
                if (!nasBlock.collides) { return false; }
                fallDamageMultiplier = nasBlock.fallDamageMultiplier;
                
                AABB blockAABB = nasBlock.bounds;
                //running down stairs will kill you at the bottom if their actual max Y is respected
                //make them taller so the server can know you're touching them
                blockAABB.Max.Y = 32;
                blockAABB = blockAABB.Offset(x * 32, y * 32, z * 32);
                if (AABB.Intersects(ref entityAABB, ref blockAABB)) {
                    //Player.Console.Message("nasblock ID is {0} and its fallDamageMultiplier is {1} and its max bound Y is {2}",
                    //                       nasBlock.selfID,
                    //                       nasBlock.fallDamageMultiplier,
                    //                       nasBlock.bounds.Max.Y
                    //                      );
                    
                    return true;
                }
                return false;
            }
        }
        
}