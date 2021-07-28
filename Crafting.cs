using System;
using System.Drawing;
using System.Collections.Generic;
using MCGalaxy;
using BlockID = System.UInt16;
using MCGalaxy.Network;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public partial class Crafting {
        public static readonly object locker = new object();
        public static void ClearCraftingArea(Player p, ushort startX, ushort startY, ushort startZ, Station.Orientation ori) {
            bool WE = ori == Station.Orientation.WE;
            if (WE) { startX--; } else { startZ--; }
            startY += 3;
            if (WE) {
                for (ushort y = startY; y > startY - 3; y--) {
                    for (ushort x = startX; x < startX + 3; x++) {
                        p.level.UpdateBlock(p, x, y, startZ, Block.Air);
                    }
                }
            } else {
                for (ushort y = startY; y > startY - 3; y--) {
                    for (ushort z = startZ; z < startZ + 3; z++) {
                        p.level.UpdateBlock(p, startX, y, z, Block.Air);
                    }
                }
            }
        }
        public class Station {
            public string name;
            public enum Type { None, Normal, Furnace, Anvil }
            public Type type = Type.None;
            /// <summary>
            /// WE goes along X axis, NS goes along Z axis
            /// </summary>
            public enum Orientation { None, WE, NS }
            public Orientation ori = Orientation.None;
            
            public Station() { }
            public Station(Station parent) {
                name = parent.name;
                type = parent.type;
                ori = parent.ori;
            }
            
            public void ShowArea(NasPlayer np, ushort x, ushort y, ushort z, Color color, int millisecs = 2000, byte A = 127) {
                //if (np.craftingAreaBeingShown) { return; }
                //np.craftingAreaBeingShown = true;
                ushort startX = x, startY = y, startZ = z;
                ushort endX = x, endY = y, endZ = z;

                bool WE = ori == Orientation.WE;
                if (WE) {
                    endX += 2;
                    startX--;
                    endZ++;
                } else {
                    endZ += 2;
                    startZ--;
                    endX++;
                }
                startY += 4;
                endY++;
                AreaInfo info = new AreaInfo();
                info.np = np;
                info.start = new Vec3U16(startX, startY, startZ);
                info.end = new Vec3U16(endX, endY, endZ);
                info.totalRounds = 16;
                info.curRound = info.totalRounds;
                info.delay = TimeSpan.FromMilliseconds(millisecs / info.totalRounds);
                info.R = color.R; info.G = color.G; info.B = color.B;
                info.A = A;
                info.ID = np.craftingAreaID++;
                SchedulerTask showAreaTask = Server.MainScheduler.QueueRepeat(ShowAreaTask, info, TimeSpan.Zero);
            }
            private class AreaInfo {
                public NasPlayer np;
                public Vec3U16 start;
                public Vec3U16 end;
                public int totalRounds;
                public int curRound;
                public TimeSpan delay;
                public short R, G, B;
                public byte A;
                public byte ID;
            }
            static void ShowAreaTask(SchedulerTask task) {
                AreaInfo info = (AreaInfo)task.State;
                if (info.curRound <= 0) {
                    info.np.p.Send(Packet.DeleteSelection(info.ID));
                    //info.np.craftingAreaBeingShown = false;
                    task.Repeating = false;
                    return;
                }
                task.Delay = info.delay;
                short alpha = (short)((info.A / info.totalRounds) * info.curRound);
                info.np.p.Send(Packet.MakeSelection(info.ID, "Crafting Zone", info.start, info.end, info.R, info.G, info.B, alpha, true));
                info.curRound--;
            }
        }
        static List<Recipe> recipes = new List<Crafting.Recipe>();

        public static Recipe GetRecipe(Player p, ushort x, ushort y, ushort z, Station station) {
            //Thread.Sleep(1000);
            NasBlock[,] area = GetArea(p, x, y, z, station.ori);
            foreach (Recipe recipe in recipes) {
                if (recipe.stationType != station.type) { continue; }
                if (recipe.shapeless) {
                    if (recipe.MatchesShapeless(area, p)) {
                        return recipe;
                    }
                } else if (recipe.Matches(area, p)) {
                    return recipe;
                }
            }
            return null;
        }
        static NasBlock[,] GetArea(Player p, ushort startX, ushort startY, ushort startZ, Station.Orientation ori) {
            NasBlock[,] area = new NasBlock[3, 3];
            bool WE = ori == Station.Orientation.WE;
            if (WE) { startX--; } else { startZ--; }
            startY += 3;
            int indexX = 0;
            int indexY = 0;
            if (WE) {
                for (ushort y = startY; y > startY - 3; y--) {
                    for (ushort x = startX; x < startX + 3; x++) {
                        BlockID blockID = p.level.GetBlock(x, y, startZ);
                        if (blockID == Block.Invalid) { blockID = 0; }
                        blockID = p.ConvertBlock(blockID);
                        NasBlock nb = NasBlock.Get(blockID);
                        //Player.Console.Message("Block at "+indexX+", "+indexY+" is "+block);
                        area[indexX, indexY] = nb;

                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            } else {
                for (ushort y = startY; y > startY - 3; y--) {
                    for (ushort z = startZ; z < startZ + 3; z++) {
                        BlockID blockID = p.level.GetBlock(startX, y, z);
                        if (blockID == Block.Invalid) { blockID = 0; }
                        blockID = p.ConvertBlock(blockID);
                        NasBlock nb = NasBlock.Get(blockID);
                        //Player.Console.Message("Block at "+indexX+", "+indexY+" is "+block);
                        area[indexX, indexY] = nb;

                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            }
            return area;
        }
        public class Recipe {
            public string name;
            public BlockID[,] pattern;
            public Dictionary<BlockID, int> patternCost {
                get {
                    Dictionary<BlockID, int> patternCost = new Dictionary<BlockID, int>();
                    for (int x = 0; x < pattern.GetLength(1); x++) {
                        for (int y = 0; y < pattern.GetLength(0); y++) {
                            BlockID curPatternID = NasBlock.Get(pattern[y, x]).parentID;
                            FillDict(curPatternID, ref patternCost);
                        }
                    }
                    return patternCost;
                }
            }
            static void FillDict(BlockID ID, ref Dictionary<BlockID, int> stacks) {
                if (ID == 0) { return; }
                if (stacks.ContainsKey(ID)) {
                    stacks[ID]++;
                } else {
                    stacks.Add(ID, 1);
                }
            }
            public Station.Type stationType = Station.Type.Normal;
            /// <summary>
            /// false to make this recipe strict (e.g. sideways stick doesn't work, only upright)
            /// and true to make this recipe work with any rotation of a block group (e.g. all four monitors work)
            /// </summary>
            public bool usesParentID = false;
            public bool shapeless = false;
            public Drop drop;
            public Recipe() {
                recipes.Add(this);
            }
            public Recipe(Item item) : this() {
                name = item.name;
                drop = new Drop(item);
            }
            public Recipe(BlockID blockID, int amount) : this() {
                name = blockID.ToString();
                drop = new Drop(blockID, amount);
            }
            public bool MatchesShapeless(NasBlock[,] area, Player p) {
                int patternWidth = pattern.GetLength(1);
                int patternHeight = pattern.GetLength(0);

                Dictionary<BlockID, int> patternStacks = new Dictionary<BlockID, int>();
                Dictionary<BlockID, int> areaStacks = new Dictionary<BlockID, int>();

                for (int patternX = 0; patternX < patternWidth; patternX++) {
                    for (int patternY = 0; patternY < patternHeight; patternY++) {
                        BlockID required = pattern[patternY, patternX];
                        FillDict(required, ref patternStacks);
                    }
                }

                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 3; y++) {
                        NasBlock suppliedNB = area[x, y];
                        BlockID supplied = usesParentID ? suppliedNB.parentID : suppliedNB.selfID;
                        FillDict(supplied, ref areaStacks);
                    }
                }

                bool matches = true;
                foreach (KeyValuePair<BlockID, int> pair in patternStacks) {
                    if (!(areaStacks.ContainsKey(pair.Key) && areaStacks[pair.Key] == pair.Value)) {
                        matches = false;
                    }
                }
                return matches;

            }
            public bool Matches(NasBlock[,] area, Player p) {
                int patternWidth = pattern.GetLength(1);
                int patternHeight = pattern.GetLength(0);

                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 3; y++) {

                        //p.Message("about to test the recipe at {0}, {1}", x, y);
                        if (TestRecipe(area, x, y, true, p) || TestRecipe(area, x, y, false, p)) {
                            //p.Message("TestRecipe is true, and we're not out of bounds");
                            // Check to make sure there aren't any sneaky unused items in the grid
                            //bounds of the current recipe
                            int minX = x, maxX = x + patternWidth;
                            int minY = y, maxY = y + patternHeight;
                            //for the entire crafting grid,
                            for (int _x = 0; _x < 3; _x++) {
                                for (int _y = 0; _y < 3; _y++) {
                                    //if a spot is outside the bounds of the current recipe,
                                    if (_x < minX || _x >= maxX || _y < minY || _y >= maxY) {
                                        if (area[_x, _y].selfID != 0) { //and the block at this spot is not air, there was an unused item and the recipe is invalid
                                            //fp.Message("%cRecipe detected but a block was outside of it");
                                            return false;
                                        }
                                    }
                                }
                            }

                            return true;
                        }

                    }
                }
                return false;
            }
            private bool TestRecipe(NasBlock[,] area, int offsetX, int offsetY, bool mirrored, Player p) {
                //the offset describes an offset down-right into the crafting grid
                int patternWidth = pattern.GetLength(1);
                int patternHeight = pattern.GetLength(0);
                //if we're out of bounds on lower-right side, it definitely doesn't match
                //p.Message("testing recipe at offset {0}, {1}", offsetX, offsetY);
                if (offsetX + patternWidth > 3 || offsetY + patternHeight > 3) {
                    //p.Message("out of bounds TestRecipe");
                    return false;
                }
                for (int x = 0; x < patternWidth; x++) {
                    for (int y = 0; y < patternHeight; y++) {

                        int xPattern = mirrored ? patternWidth - 1 - x : x;
                        //p.Message("index of xPattern is {0}, {1}", xPattern, y);

                        NasBlock suppliedNB = area[x + offsetX, y + offsetY];
                        BlockID supplied = usesParentID ? suppliedNB.parentID : suppliedNB.selfID;
                        BlockID required = pattern[y, xPattern];
                        //p.Message("supplied XY {0}{1}, required XY {2}{3}", x+offsetX, y+offsetY, x, y);
                        //p.Message("supplied is {0}, required is {1}", supplied, required);
                        if (supplied != required) {
                            //p.Message("supplied isnt required");
                            return false;
                        }
                    }
                }
                //p.Message("tested success");
                return true;
            }


        } //Recipe


    }

}
