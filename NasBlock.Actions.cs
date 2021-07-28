using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using BlockID = System.UInt16;
using NasBlockAction = System.Action<NotAwesomeSurvival.NasLevel, int, int, int>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
        static NasBlockAction FloodAction(BlockID[] set) {
            return (nl,x,y,z) => {
                if (CanInfiniteFloodKillThis(nl, x, y-1, z, set) ) {
                    nl.SetBlock(x, y-1, z, set[LiquidInfiniteIndex]);
                    return;
                }
                if (CanInfiniteFloodKillThis(nl, x+1, y, z, set) ) {
                    nl.SetBlock(x+1, y, z, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x-1, y, z, set) ) {
                    nl.SetBlock(x-1, y, z, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x, y, z+1, set) ) {
                    nl.SetBlock(x, y, z+1, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x, y, z-1, set) ) {
                    nl.SetBlock(x, y, z-1, set[LiquidInfiniteIndex]);
                }
            };
        }
        static bool CanInfiniteFloodKillThis(NasLevel nl, int x, int y, int z, BlockID[] set) {
            BlockID here = nl.GetBlock(x, y, z);
            if (CanPhysicsKillThis(here) || IsPartOfSet(set, here) > LiquidInfiniteIndex) { return true; }
            return false;
        }
        
        public static BlockID[] blocksPhysicsCanKill = new BlockID[] {
            0,
            39,
            40,
            Block.Extended|130,
            Block.FromRaw(644), Block.FromRaw(645), Block.FromRaw(646), Block.FromRaw(461)
        };
        public static bool CanPhysicsKillThis(BlockID block) {
            for (int i = 0; i < blocksPhysicsCanKill.Length; i++) {
                if (block == blocksPhysicsCanKill[i]) { return true; }
            }
            return false;
        }
        public static bool IsThisLiquid(BlockID block) {
            if (IsPartOfSet(waterSet, block) != -1) { return true; }
            return false;
        }
        
        static int LiquidInfiniteIndex = 0;
        static int LiquidSourceIndex = 1;
        static int LiquidWaterfallIndex = 2;
        /// <summary>
        /// First ID is the infinite-flood version of the liquid, second is the source, third is waterfall, the rest are heights from tallest to shortest
        /// </summary>
        public static BlockID[] waterSet = new BlockID[] { 8, 9, Block.Extended|639,
            Block.Extended|632,
            Block.Extended|633,
            Block.Extended|634,
            Block.Extended|635,
            Block.Extended|636,
            Block.Extended|637,
            Block.Extended|638 };
        
        /// <summary>
        /// Check if the given block exists within the given set.
        /// </summary>
        /// <returns>The index of the set that the block is at
        /// or -1 if the block does not exist within the set.
        /// </returns>
        public static int IsPartOfSet(BlockID[] set, BlockID block) {
            for (int i = 0; i < set.Length; i++) {
                if (set[i] == block) { return i; }
            }
            return -1;
        }
        /// <summary>
        /// returns -1 if not part of the set, spreadIndex+1 if air(or block liquids kill), otherwise the index into the set
        /// </summary>
        static int CanReplaceBlockAt(NasLevel nl, int x, int y, int z, BlockID[] set, int spreadIndex) {
            BlockID hereBlock = nl.GetBlock(x, y, z);
            if (CanPhysicsKillThis(hereBlock)) { return spreadIndex+1; }
            
            int hereIndex = IsPartOfSet(set, hereBlock);
            return hereIndex;
        }
        static bool CanLiquidLive(NasLevel nl, BlockID[] set, int index, int x, int y, int z) {
            BlockID neighbor = nl.GetBlock(x, y, z);
            if (neighbor == set[index-1] ||
                neighbor == set[LiquidSourceIndex] ||
                neighbor == set[LiquidWaterfallIndex]
               ) {
                return true;
            }
            return false;
        }
        
        static NasBlockAction LimitedFloodAction(BlockID[] set, int index) {
            return (nl,x,y,z) => {
                //Step one -- Check if we need to drain
                if (index > LiquidSourceIndex) {
                    //it's not a source block
                    
                    if (index == LiquidWaterfallIndex) {
                        //it's a waterfall -- see if it needs to die
                        BlockID aboveHere = nl.GetBlock(x, y+1, z);
                        if (IsPartOfSet(set, aboveHere) == -1) {
                            //nl.lvl.Message("killing waterfall");
                            nl.SetBlock(x, y, z, Block.Air);
                            return;
                        }
                    } else {
                        //it's not a waterfall -- see if it needs to die
                        if (!(CanLiquidLive(nl, set, index, x+1, y, z) ||
                            CanLiquidLive(nl, set, index, x-1, y, z) ||
                            CanLiquidLive(nl, set, index, x, y, z+1) ||
                            CanLiquidLive(nl, set, index, x, y, z-1)) ) {
                            
                            //nl.lvl.Message("killing liquid");
                            nl.SetBlock(x, y, z, Block.Air);
                            return;
                        }
                    }
                }
                
                //Step two -- Do the actual flooding
                
                BlockID below = nl.GetBlock(x, y-1, z);
                int belowIndex = IsPartOfSet(set, below);
                if (CanPhysicsKillThis(below) || belowIndex != -1) {
                    //don't override infinite source, source, or waterfall with a waterfall
                    if (!CanPhysicsKillThis(below) && belowIndex <= LiquidWaterfallIndex) { return; }
                    
                    //nl.lvl.Message("setting waterfall");
                    nl.SetBlock(x, y-1, z, set[LiquidWaterfallIndex]);
                    return;
                }
                
                if (index == set.Length-1) {
                    //it's the end of the stream -- no need to flood further
                    return;
                }
                
                int spreadIndex = (index < LiquidWaterfallIndex+1) ? LiquidWaterfallIndex+1 : index+1;
                BlockID spreadBlock = set[spreadIndex];
                
                bool posX;
                bool negX;
                bool posZ;
                bool negZ;
                CanFlowInDirection(nl, x, y, z, set, spreadIndex,
                                   out posX,
                                   out negX,
                                   out posZ,
                                   out negZ);
                
                if (posX) {
                    nl.SetBlock(x+1, y, z, spreadBlock);
                }
                if (negX) {
                    nl.SetBlock(x-1, y, z, spreadBlock);
                }
                if (posZ) {
                    nl.SetBlock(x, y, z+1, spreadBlock);
                }
                if (negZ) {
                    nl.SetBlock(x, y, z-1, spreadBlock);
                }
                
                
            };
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        static void CanFlowInDirection(NasLevel nl, int x, int y, int z,
                                       BlockID[] set, int spreadIndex,
                                       out bool xPos,
                                       out bool xNeg,
                                       out bool zPos,
                                       out bool zNeg
                                      ) {
            xPos = true;
            xNeg = true;
            zPos = true;
            zNeg = true;
            
            bool xBlockedPos = false;
            bool xBlockedNeg = false;
            bool zBlockedPos = false;
            bool zBlockedNeg = false;
            
            
            int originalHoleDistance;
            List<Vec3S32> holes = HolesInRange(nl, x, y, z, 4, set, out originalHoleDistance);
            if (holes.Count > 0) {
                CloserToAHole(x, y, z,  1,  0, originalHoleDistance, holes, ref xPos);
                CloserToAHole(x, y, z, -1,  0, originalHoleDistance, holes, ref xNeg);
                CloserToAHole(x, y, z,  0,  1, originalHoleDistance, holes, ref zPos);
                CloserToAHole(x, y, z,  0, -1, originalHoleDistance, holes, ref zNeg);
            }
            
            int neighborIndex1 = CanReplaceBlockAt(nl, x+1, y, z, set, spreadIndex);
            int neighborIndex2 = CanReplaceBlockAt(nl, x-1, y, z, set, spreadIndex);
            int neighborIndex3 = CanReplaceBlockAt(nl, x, y, z+1, set, spreadIndex);
            int neighborIndex4 = CanReplaceBlockAt(nl, x, y, z-1, set, spreadIndex);
            
            if (neighborIndex1 == -1) {
                xBlockedPos = true;
            }
            if (neighborIndex2 == -1) {
                xBlockedNeg = true;
            }
            if (neighborIndex3 == -1) {
                zBlockedPos = true;
            }
            if (neighborIndex4 == -1) {
                zBlockedNeg = true;
            }
            xPos = xPos && !xBlockedPos;
            xNeg = xNeg && !xBlockedNeg;
            zPos = zPos && !zBlockedPos;
            zNeg = zNeg && !zBlockedNeg;
            
            if (!(xPos || xNeg || zPos || zNeg)) { //no water can be spread
                //allow any to spread that were not blocked by solid blocks before
                xPos = !xBlockedPos;
                xNeg = !xBlockedNeg;
                zPos = !zBlockedPos;
                zNeg = !zBlockedNeg;
                
            }
            //make it not spread if the neighbor is taller
            xPos = xPos && neighborIndex1 > spreadIndex;
            xNeg = xNeg && neighborIndex2 > spreadIndex;
            zPos = zPos && neighborIndex3 > spreadIndex;
            zNeg = zNeg && neighborIndex4 > spreadIndex;
            
            
        }
        static void CloserToAHole(int x, int y, int z, int xDiff, int zDiff, int originalHoleDistance, List<Vec3S32> holes, ref bool canFlowDir) {
            x += xDiff;
            z += zDiff;
            foreach (var hole in holes) {
                int dist = Math.Abs(x - hole.X) + Math.Abs(z - hole.Z);
                if (dist < originalHoleDistance) {
                    canFlowDir = true; return;
                }
            }
            canFlowDir = false;
        }
        
        
        public class FloodSim {
            NasLevel nl;
            int xO;
            int yO;
            int zO;
            int totalDistance;
            BlockID[] liquidSet;
            bool[,] waterAtSpot;
            int widthAndHeight;
            
            List<Vec3S32> holes;
            int distanceHolesWereFoundAt;
            
            
            public FloodSim(NasLevel nl, int xO, int yO, int zO, int totalDistance, BlockID[] set) {
                this.nl = nl;
                this.xO = xO;
                this.yO = yO;
                this.zO = zO;
                this.totalDistance = totalDistance;
                this.liquidSet = set;
                waterAtSpot = new bool[totalDistance*2+1,totalDistance*2+1];
                widthAndHeight = waterAtSpot.GetLength(0);
                
                holes = new List<Vec3S32>();
                distanceHolesWereFoundAt = totalDistance;
            }
            public List<Vec3S32> GetHoles(out int distance) {
                //place water in the center
                Flood(xO, zO, true);
                TryFlood(xO+1, yO, zO);
                TryFlood(xO-1, yO, zO);
                TryFlood(xO,   yO, zO+1);
                TryFlood(xO,   yO, zO-1);
                
                distance = distanceHolesWereFoundAt;
                return holes;
            }
            void TryFlood(int x, int y, int z) {
                int distanceFromCenter = Math.Abs(x - xO) + Math.Abs(z - zO);
                //this spot is out of bounds? quit
                if (distanceFromCenter > totalDistance) {
                    return;
                }
                //this spot has been flooded already? quit
                if (AlreadyFlooded(x, z)) {
                    return;
                }
                
                BlockID here = nl.GetBlock(x, y, z);
                //can't flood into this spot? quit
                if (!(CanPhysicsKillThis(here) || IsPartOfSet(liquidSet, here) != -1) ) {
                    return;
                }
                BlockID below = nl.GetBlock(x, y-1, z);
                //if there's a hole here
                if (CanPhysicsKillThis(below) || IsPartOfSet(liquidSet, below) != -1) {
                    if (distanceFromCenter < distanceHolesWereFoundAt) {
                        holes.Clear();
                        holes.Add(new Vec3S32(x, y-1, z));
                        distanceHolesWereFoundAt = distanceFromCenter;
                    } else if (distanceFromCenter == distanceHolesWereFoundAt) {
                        holes.Add(new Vec3S32(x, y-1, z));
                    }
                }
                Flood(x, z, true);
                TryFlood(x+1, y, z);
                TryFlood(x-1, y, z);
                TryFlood(x, y, z+1);
                TryFlood(x, y, z-1);
            }
            bool AlreadyFlooded(int x, int z) {
                int xI = x - xO;
                int zI = z - zO;
                xI += totalDistance;
                zI += totalDistance;
                //both dimensions are the same 
                if (
                    xI >= widthAndHeight ||
                    zI >= widthAndHeight ||
                    xI <  0 ||
                    zI <  0
                   ) {
                    return false;
                }
                return waterAtSpot[xI,zI];
            }
            void Flood(int x, int z, bool value) {
                int xI = x - xO;
                int zI = z - zO;
                xI += totalDistance;
                zI += totalDistance;
                
                waterAtSpot[xI,zI] = value;
            }
        }
        public static List<Vec3S32> HolesInRange(NasLevel nl, int x, int y, int z, int totalDistance, BlockID[] set, out int distance) {
            FloodSim sim = new FloodSim(nl, x, y, z, totalDistance, set);
            return sim.GetHoles(out distance);
        }
        
        static NasBlockAction FallingBlockAction(BlockID serverBlockID) {
            return (nl,x,y,z) => {
                BlockID blockUnder = nl.GetBlock(x, y-1, z);
                if (CanPhysicsKillThis(blockUnder) || IsPartOfSet(waterSet, blockUnder) != -1) {
                    nl.SetBlock(x, y, z, Block.Air);
                    nl.SetBlock(x, y-1, z, serverBlockID);
                }
            };
        }
        static NasBlockAction GrassBlockAction(BlockID grass, BlockID dirt) {
            return (nl,x,y,z) => {
                BlockID aboveHere = nl.GetBlock(x, y+1, z);
                if (!nl.lvl.LightPasses(aboveHere)) {
                    nl.SetBlock(x, y, z, dirt);
                }
            };
        }
        static BlockID[] grassSet = new BlockID[] { Block.Grass, Block.Extended|119, Block.Extended|129 };
        static BlockID[] tallGrassSet = new BlockID[] { 40, Block.Extended|120, Block.Extended|130 };
        static NasBlockAction DirtBlockAction(BlockID[] grassSet, BlockID dirt) {
            return (nl,x,y,z) => {
                BlockID aboveHere = nl.GetBlock(x, y+1, z);
                if (!nl.lvl.LightPasses(aboveHere)) {
                    //nl.lvl.Message("Can't grow since solid above");
                    return;
                }
                
                for (int xOff = -1; xOff <= 1; xOff++)
                    for (int yOff = -1; yOff <= 1; yOff++)
                        for (int zOff = -1; zOff <= 1; zOff++)
                {
                    if (xOff == 0 && yOff == -1 && zOff == 0) { continue; }
                    BlockID neighbor = nl.GetBlock(x+xOff, y+yOff, z+zOff);
                    int setIndex = IsPartOfSet(grassSet, neighbor);
                    if (setIndex == -1) { continue; }
                    nl.SetBlock(x, y, z, grassSet[setIndex], true);
                    if (nl.GetBlock(x, y+1, z) == Block.Air && r.Next(0, 2) == 0) {
                        nl.SetBlock(x, y+1, z, tallGrassSet[setIndex]);
                    }
                    return;
                }
            };
        }
		private static Random CoordRandom(int x, int y, int z) {
			string rndString = x+" "+y+" "+z;
			return new Random(rndString.GetHashCode()); 
		}
        
        
        static BlockID[] logSet = new BlockID[] { 15, 16, 17, Block.Extended|144 };
        static NasBlockAction LeafBlockAction(BlockID[] logSet, BlockID leaf) {
            return (nl,x,y,z) => {
                bool canLive = false;
                int iteration = 1;
                IsThereLog(nl, x+1, y,   z,   leaf, iteration, ref canLive);
                IsThereLog(nl, x,   y+1, z,   leaf, iteration, ref canLive);
                IsThereLog(nl, x,   y,   z+1, leaf, iteration, ref canLive);
                IsThereLog(nl, x-1, y,   z,   leaf, iteration, ref canLive);
                IsThereLog(nl, x,   y-1, z,   leaf, iteration, ref canLive);
                IsThereLog(nl, x,   y,   z-1, leaf, iteration, ref canLive);
                if (canLive) {
                    //Player.Console.Message("It can live!");
                    return;
                }
                nl.SetBlock(x, y, z, Block.Air);
                if (r.Next(0, 384) == 0 && CanPhysicsKillThis(nl.GetBlock(x, y-1, z)) ) {
                    nl.SetBlock(x, y-1, z, Block.FromRaw(648));
                }
            };
        }
        static void IsThereLog(NasLevel nl, int x, int y, int z, BlockID leaf, int iteration, ref bool canLive) {
            if (canLive) { return; }
            BlockID hereBlock = nl.GetBlock(x, y, z);
            if (IsPartOfSet(logSet, hereBlock) != -1) {
                canLive = true;
                return;
            }
            if (hereBlock != leaf) { return; }
            if (iteration >= 5) { return; }
            iteration++;
            IsThereLog(nl, x+1, y,   z,   leaf, iteration, ref canLive);
            IsThereLog(nl, x,   y+1, z,   leaf, iteration, ref canLive);
            IsThereLog(nl, x,   y,   z+1, leaf, iteration, ref canLive);
            IsThereLog(nl, x-1, y,   z,   leaf, iteration, ref canLive);
            IsThereLog(nl, x,   y-1, z,   leaf, iteration, ref canLive);
            IsThereLog(nl, x,   y,   z-1, leaf, iteration, ref canLive);
        }
        
        
        static NasBlockAction NeedsSupportAction() {
            return (nl,x,y,z) => {
                IsSupported(nl, x, y, z);
            };
        }
        static NasBlockAction GenericPlantAction() {
            return (nl,x,y,z) => {
                GenericPlantSurvived(nl, x, y, z);
            };
        }
        
        public static BlockID[] leafSet = new BlockID[] { Block.Leaves };
        static NasBlockAction OakSaplingAction() {
            return (nl,x,y,z) => {
                if (!GenericPlantSurvived(nl, x, y, z)) { return; }
                nl.SetBlock(x, y, z, Block.Air);
                NasTree.GenOakTree(nl, r, x, y, z, true);
            };
        }
        
        static BlockID[] wheatSet = new BlockID[] { Block.FromRaw(644), Block.FromRaw(645), Block.FromRaw(646), Block.FromRaw(461) };
        static NasBlockAction CropAction(BlockID[] cropSet, int index) {
            return (nl,x,y,z) => {
                if (!CropSurvived(nl, x, y, z)) { return; }
                if (index+1 >= cropSet.Length) { return; }
                nl.SetBlock(x, y, z, cropSet[index+1]);
            };
        }
        
        
        
        static bool IsSupported(NasLevel nl, int x, int y, int z) {
            BlockID below = nl.GetBlock(x, y-1, z);
            if (CanPhysicsKillThis(below)) {
                nl.SetBlock(x, y, z, Block.Air);
                return false;
            }
            return true;
        }
        static bool GenericPlantSurvived(NasLevel nl, int x, int y, int z) {
            if (!IsSupported(nl, x, y, z)) { return false; }
            if (!CanPlantsLiveOn(nl.GetBlock(x, y-1, z))) {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        static bool CropSurvived(NasLevel nl, int x, int y, int z) {
            if (!IsSupported(nl, x, y, z)) { return false; }
            if (IsPartOfSet(soilForPlants, nl.GetBlock(x, y-1, z)) == -1 ) {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        
        static BlockID[] soilForPlants = new BlockID[] { Block.Dirt, Block.Extended|144 };
        static bool CanPlantsLiveOn(BlockID block) {
            if (IsPartOfSet(soilForPlants, block) != -1 || IsPartOfSet(grassSet, block) != -1) {
                return true;
            }
            return false;
        }

        
        
    }

}
