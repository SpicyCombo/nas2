using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;
using NasBlockInteraction =
    System.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
using NasBlockExistAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
            
            
            public class Entity {
                public bool CanAccess(Player p) {
                    return lockedBy.Length == 0 || lockedBy == p.name;
                }
                [JsonIgnore] public string FormattedNameOfLocker {
                    get {
                        if (lockedBy.Length == 0) { return "no one"; }
                        Player locker = PlayerInfo.FindExact(lockedBy);
                        return locker == null ? lockedBy : locker.ColoredName;
                    } }
                public string lockedBy = "";
                public Drop drop = null;
            }
            
            public class Container {
                public const int ToolLimit = 9;
                public const int BlockStackLimit = 9;
                public static readonly object locker = new object();
                public enum Type { Chest, Barrel, Crate, Gravestone }
                public Type type;
                public string name { get { return Type.GetName(typeof(Type), type); } }
                public string description { get {
                        string desc = "%s";
                        switch (type) {
                            case NasBlock.Container.Type.Chest:
                                desc += name+"s%S can store %btools%S, with a limit of "+ToolLimit+".";
                                break;
                            case NasBlock.Container.Type.Barrel:
                            case NasBlock.Container.Type.Crate:
                                desc += name+"s%S can store %bblock%S stacks, with a limit of "+BlockStackLimit+".";
                                break;
                            default:
                                throw new Exception("Invalid value for Type");
                        }
                        return desc;
                    } }
                public Container() { }
                public Container(Container parent) {
                    type = parent.type;
                }
            }
            
            static NasBlockExistAction WaterExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    if (exists) {
                        //give back barrel
                        np.inventory.SetAmount(143, 1, false, false);
                    }
                };
            }
            
            static NasBlockInteraction CrateInteraction() {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    np.p.Message("You can't open it. It's just for decoration.");
                };
            }
            
            static NasBlockExistAction ContainerExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    //this is voodoo -- the question is, is it too much voodoo for the next 10 centuries for god's official temple?
                    if (exists && nasBlock.container.type == Container.Type.Barrel) {
                        if (
                            NasBlock.IsPartOfSet(NasBlock.waterSet, np.nl.GetBlock(x, y+1, z)) != -1 ||
                            NasBlock.IsPartOfSet(NasBlock.waterSet, np.nl.GetBlock(x+1, y, z)) != -1 ||
                            NasBlock.IsPartOfSet(NasBlock.waterSet, np.nl.GetBlock(x-1, y, z)) != -1 ||
                            NasBlock.IsPartOfSet(NasBlock.waterSet, np.nl.GetBlock(x, y, z+1)) != -1 ||
                            NasBlock.IsPartOfSet(NasBlock.waterSet, np.nl.GetBlock(x, y, z-1)) != -1
                           ) {
                            np.nl.SetBlock(x, y, z, Block.Air);
                            //water barrel
                            np.inventory.SetAmount(643, 1, true, true);
                            np.inventory.DisplayHeldBlock(NasBlock.blocks[143], -1, false);
                            return;
                        }
                    }
                    
                    lock (Container.locker) {
                        if (exists) {
                            if (nasBlock.container.type == Container.Type.Gravestone) {
                                //np.p.Message("do nothing");
                                return;
                            }
                            
                            //Entity blockEntity = new Entity(); ??????????????????????????????????????????????????????
                            if (np.nl.blockEntities.ContainsKey(x+" "+y+" "+z)) {
                                //np.p.Message("You just overrode a spot that used to contain a chest.");
                                np.nl.blockEntities.Remove(x+" "+y+" "+z);
                            }
                            np.nl.blockEntities.Add(x+" "+y+" "+z, new Entity());
                            //np.p.Message("You placed a {0}!", nasBlock.container.name);
                            np.p.Message(nasBlock.container.description);
                            np.p.Message("To insert, select what you want to store, then left click.");
                            np.p.Message("To extract, right click.");
                            np.p.Message("To inspect status, middle click.");
                            return;
                        }
                        
                        np.nl.blockEntities.Remove(x+" "+y+" "+z);
                        //np.p.Message("You destroyed a {0}!", nasBlock.container.name);
                    }
                };
            }
            
            static NasBlockInteraction ContainerInteraction() {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    lock (Container.locker) {
                        if (np.nl.blockEntities.ContainsKey(x+" "+y+" "+z)) {
                            Entity bEntity = np.nl.blockEntities[x+" "+y+" "+z];
                            
                            if (!bEntity.CanAccess(np.p)) {
                                np.p.Message("This {0} is locked by {1}%S.", nasBlock.container.name.ToLower(), bEntity.FormattedNameOfLocker);
                                return;
                            }
                            
                            //np.p.Message("There is a blockEntity here.");
                            if (button == MouseButton.Middle) {
                                CheckContents(np, nasBlock, bEntity);
                                return;
                            }
                            
                            if (button == MouseButton.Left) {

                                if (np.inventory.HeldItem.name == "Key") {
                                    if (nasBlock.container.type == Container.Type.Gravestone) {
                                        np.p.Message("You cannot lock gravestones.");
                                    }
                                    //it's already unlocked, lock it
                                    else if (bEntity.lockedBy.Length == 0) {
                                        bEntity.lockedBy = np.p.name;
                                        np.p.Message("You %flock%S the {0}. Only you can access it now.", nasBlock.container.name.ToLower());
                                        return;
                                    }
                                }
                                
                                if (nasBlock.container.type == Container.Type.Gravestone) {
                                    np.p.Message("You can right click to extract from tombstones.");
                                    return;
                                }
                                
                                if (nasBlock.container.type == Container.Type.Chest) {
                                    AddTool(np, bEntity);
                                } else {
                                    AddBlocks(np, bEntity);
                                }
                                return;
                            }
                            
                            if (button == MouseButton.Right) {
                                if (np.inventory.HeldItem.name == "Key") {
                                    //it's locked, unlock it
                                    if (bEntity.lockedBy.Length > 0) {
                                        bEntity.lockedBy = "";
                                        np.p.Message("You %funlock%S the {0}. Anyone can access it now.", nasBlock.container.name.ToLower());
                                        return;
                                    }
                                }
                                
                                if (nasBlock.container.type == Container.Type.Chest) {
                                    RemoveTool(np, bEntity);
                                } else if (nasBlock.container.type == Container.Type.Barrel) {
                                    RemoveBlocks(np, bEntity);
                                } else if (nasBlock.container.type == Container.Type.Gravestone) {
                                    RemoveAll(np, bEntity);
                                }
                                return;
                            }
                            return;
                        }
                        if (nasBlock.container.type != Container.Type.Gravestone) {
                            np.p.Message("(BUG) The data inside this {0} was lost, but you can make it functional again by %cdeleting%S then %breplacing%S it.",
                                         nasBlock.container.name.ToLower());
                        }
                    }
                };
            }
            static void AddTool(NasPlayer np, Entity bEntity) {
                if (bEntity.drop != null && bEntity.drop.items.Count >= Container.ToolLimit) {
                    np.p.Message("There can only be {0} tools at most in a chest.", Container.ToolLimit);
                    return;
                }
                if (np.inventory.items[np.inventory.selectedItemIndex] == null) {
                    np.p.Message("You need to select a tool to insert it.");
                    return;
                }
                if (bEntity.drop == null) {
                    bEntity.drop = new Drop(np.inventory.items[np.inventory.selectedItemIndex]);
                } else {
                    bEntity.drop.items.Add(np.inventory.items[np.inventory.selectedItemIndex]);
                }
                np.p.Message("You put {0}%S in the chest.", np.inventory.items[np.inventory.selectedItemIndex].ColoredName);
                np.inventory.items[np.inventory.selectedItemIndex] = null;
                np.inventory.UpdateItemDisplay();
            }
            static void RemoveTool(NasPlayer np, Entity bEntity) {
                if (bEntity.drop == null) {
                    np.p.Message("There's no tools to extract.");
                    return;
                }
                Drop taken = new Drop(bEntity.drop.items[bEntity.drop.items.Count-1]);
                bEntity.drop.items.RemoveAt(bEntity.drop.items.Count-1);
                np.inventory.GetDrop(taken, true);
                if (bEntity.drop.items.Count == 0) {
                    bEntity.drop = null;
                }
            }
            
            
            
            
            static void AddBlocks(NasPlayer np, Entity bEntity) {
                Player p = np.p;
                //p.ClientHeldBlock is server block ID
                BlockID clientBlockID = p.ConvertBlock(p.ClientHeldBlock);
                NasBlock nasBlock = NasBlock.Get(clientBlockID);
                if (nasBlock.parentID == 0) {
                    p.Message("Select a block to store it.");
                    return;
                }
                int amount = np.inventory.GetAmount(nasBlock.parentID);
                
                if (amount < 1) {
                    p.Message("You don't have any {0} to store.", nasBlock.GetName(p));
                    return;
                }
                
                if (amount > 3) { amount /= 2; }
                
                
                if (bEntity.drop == null) {
                    np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                    bEntity.drop = new Drop(nasBlock.parentID, amount);
                    return;
                }
                foreach (BlockStack stack in bEntity.drop.blockStacks) {
                    //if a stack exists in the container already, add to that stack
                    if (stack.ID == nasBlock.parentID) {
                        np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                        stack.amount += amount;
                        return;
                    }
                }
                
                if (bEntity.drop.blockStacks.Count >= Container.BlockStackLimit) {
                    p.Message("It can't contain more than {0} stacks of blocks.", Container.BlockStackLimit);
                    return;
                }
                np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                bEntity.drop.blockStacks.Add(new BlockStack(nasBlock.parentID, amount));
                
                
            }
            static void RemoveBlocks(NasPlayer np, Entity bEntity) {
                Player p = np.p;
                if (bEntity.drop != null && bEntity.drop.blockStacks != null) {
                    if (bEntity.drop.blockStacks.Count == 0) {
                        p.Message("%cTHERE ARE 0 BLOCK STACKS INSIDE WARNING THIS SHOULD NEVER HAPPEN IT SHOULD BE NULL INSTEAD");
                        return;
                    }
                    
                    BlockStack bs = null;
                    //p.ClientHeldBlock is server block ID
                    BlockID clientBlockID = p.ConvertBlock(p.ClientHeldBlock);
                    NasBlock nasBlock = NasBlock.Get(clientBlockID);
                    foreach (BlockStack stack in bEntity.drop.blockStacks) {
                        //if there's a stack in the container that matches what we're holding
                        if (stack.ID == nasBlock.parentID) {
                            //p.Message("found you");
                            bs = stack;
                            break;
                        }
                    }
                    if (bs == null) {
                        //p.Message("we didn't find a stack that matches held block, take the last one");
                        bs = bEntity.drop.blockStacks[bEntity.drop.blockStacks.Count-1];
                    }

                    int amount = bs.amount;
                    //if (amount > 3) { amount /= 2; }
                    
                    np.inventory.SetAmount(bs.ID, amount, true, true);
                    if (amount >= bs.amount) {
                        bEntity.drop.blockStacks.Remove(bs);
                    } else {
                        bs.amount -= amount;
                    }
                    
                    if (bEntity.drop.blockStacks.Count == 0) {
                        bEntity.drop = null;
                    }
                    return;
                }
                p.Message("There's no blocks to extract.");
            }
            
            static void RemoveAll(NasPlayer np, Entity bEntity) {
                Player p = np.p;
                if (bEntity.drop != null) {
                    bEntity.drop = np.inventory.GetDrop(bEntity.drop);
                    return;
                }
                p.Message("There's nothing to extract.");
            }
            
            
            
            static void CheckContents(NasPlayer np, NasBlock nb, Entity blockEntity) {
                if (blockEntity.drop == null) {
                    np.p.Message("There's nothing inside.");
                }
                else {
                    if (blockEntity.drop.items != null) {
                        np.p.Message("There's {0} tool{1} inside.", blockEntity.drop.items.Count, blockEntity.drop.items.Count == 1 ? "" : "s");
                    }
                    if (blockEntity.drop.blockStacks != null) {
                        foreach (BlockStack bs in blockEntity.drop.blockStacks) {
                            np.p.Message("There's %f{0} {1}%S inside.", bs.amount, NasBlock.blocks[bs.ID].GetName(np.p));
                        }
                    }
                }
                if (nb.container.type == Container.Type.Gravestone) { return; }
                np.p.Message("%r(%fi%r)%S This {0} is %f{1}%S by you.", nb.container.name.ToLower(), blockEntity.lockedBy.Length > 0 ? "locked" : "not locked");
            }
            
            
            
            
            
            
            
            static NasBlockExistAction CraftingExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    if (exists) {
                        np.p.Message("You placed a %b{0}%S!", nasBlock.station.name);
                        np.p.Message("Click to craft.");
                        np.p.Message("Right click to auto-replace recipe.");
                        np.p.Message("Left click for one-and-done.");
                        nasBlock.station.ShowArea(np, x, y, z, Color.White);
                        return;
                    }
                    //np.p.Message("You destroyed a {0}!", nasBlock.station.name);
                };
            }
            static NasBlockInteraction CraftingInteraction() {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    lock (Crafting.locker) {
                        Crafting.Recipe recipe = Crafting.GetRecipe(np.p, x, y, z, nasBlock.station);
                        if (recipe == null) {
                            nasBlock.station.ShowArea(np, x, y, z, Color.Red, 500, 127);
                            return;
                        }
                        Drop dropClone = new Drop(recipe.drop);
                        
                        if (np.inventory.GetDrop(dropClone, true) != null) {
                            //non null means the player couldn't fit this drop in their inventory
                            return;
                        }
                        nasBlock.station.ShowArea(np, x, y, z, Color.LightGreen, 500);
                        bool clearCraftingArea = button == MouseButton.Left;
                        var patternCost = recipe.patternCost;
                        foreach (KeyValuePair<BlockID, int> pair in patternCost) {
                            if (np.inventory.GetAmount(pair.Key) < pair.Value) {
                                clearCraftingArea = true; break;
                            }
                        }
                        if (clearCraftingArea) {
                            Crafting.ClearCraftingArea(np.p, x, y, z, nasBlock.station.ori);
                        } else {
                            foreach (KeyValuePair<BlockID, int> pair in patternCost) {
                                np.inventory.SetAmount(pair.Key, -pair.Value, false);
                            }
                        }
                    }
                };
            }
            
            
            
            static NasBlockExistAction PlantExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    if (exists) {
                        
                        return;
                    }
                    
                };
            }
            
            static BlockID[] breadSet = new BlockID[] { Block.Extended|640, Block.Extended|641, Block.Extended|642 };
            static NasBlockInteraction EatInteraction(BlockID[] set, int index, float healthRestored, float chewSeconds = 2) {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    
                    lock (Container.locker) {
                        //if (np.HP >= NasEntity.maxHP) {
                        //    np.p.Message("You're full!");
                        //    return;
                        //}
                        
                        if (np.isChewing) {
                            //np.p.Message("You're still chewing.");
                            return;
                        }
                        np.isChewing = true;
                        SchedulerTask taskChew;
                        EatInfo eatInfo = new EatInfo();
                        eatInfo.np = np;
                        eatInfo.healthRestored = healthRestored;
                        taskChew = Server.MainScheduler.QueueOnce(CanEatAgainCallback, eatInfo, TimeSpan.FromSeconds(chewSeconds));
                        
                        
                        np.p.Message("*munch*");
                        if (index == set.Length-1) {
                            np.nl.SetBlock(x, y, z, Block.Air);
                            return;
                        }
                        np.nl.SetBlock(x, y, z, set[index+1]);
                    }
                };
            }
            public class EatInfo {
                public NasPlayer np;
                public float healthRestored;
            }
            static void CanEatAgainCallback(SchedulerTask task) {
                EatInfo eatInfo = (EatInfo)task.State;
                NasPlayer np = eatInfo.np;
                float healthRestored = eatInfo.healthRestored;
                
                float HPafterHeal = np.HP + healthRestored;
                if (HPafterHeal > NasEntity.maxHP) {
                    healthRestored = NasEntity.maxHP - np.HP;
                }
                np.ChangeHealth(healthRestored);
                
                np.isChewing = false;
                np.p.Message("*gulp*");
                np.p.Message("%a+{0} %f[{1}] HP ♥", healthRestored, np.HP);
            }
        
    }

}
