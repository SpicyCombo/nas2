using BlockID = System.UInt16;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using Newtonsoft.Json;

namespace NotAwesomeSurvival {

    public partial class Inventory {

        private Player p;
        public int[] blocks = new int[Block.MaxRaw + 1];

        public Inventory(Player p) { this.p = p; }
        public void SetPlayer(Player p) { this.p = p; }
        public void Setup() {
            Player.Console.Message("setting up inventory");
            //hide all blocks
            for (BlockID clientBlockID = 1; clientBlockID <= Block.MaxRaw; clientBlockID++) {
                p.Send(Packet.BlockPermission(clientBlockID, false, false, true));
                p.Send(Packet.SetInventoryOrder(clientBlockID, 0, true));
            }
            //unhide blocks you have access to
            for (BlockID clientBlockID = 1; clientBlockID <= Block.MaxRaw; clientBlockID++) {
                if (GetAmount(clientBlockID) > 0) {
                    UnhideBlock(clientBlockID);
                }
            }
            SetupItems();
        }
        public void ClearHotbar() {
            for (byte i = 0; i <= 9; i++) {
                p.Send(Packet.SetHotbar(0, i, true));
            }
        }

        /// <summary>
        /// Returns a drop that contains the items the player was unable to pickup due to full inventory. If the drop is null, the player fit everything.
        /// </summary>
        public Drop GetDrop(Drop drop, bool showToNormalChat = false) {
            if (drop == null) { return null; }
            if (drop.blockStacks != null) {
                for (int i = 0; i < drop.blockStacks.Count; i++) {
                    BlockStack bs = drop.blockStacks[i];

                    SetAmount(bs.ID, bs.amount, false);

                    DisplayInfo info = new DisplayInfo();
                    info.inv = this;
                    info.nasBlock = NasBlock.Get(bs.ID);
                    info.amountChanged = bs.amount;
                    if (drop.blockStacks.Count == 1) {
                        info.showToNormalChat = showToNormalChat;
                    } else {
                        info.showToNormalChat = true;
                    }
                    SchedulerTask taskDisplayHeldBlock;
                    taskDisplayHeldBlock = Server.MainScheduler.QueueOnce(DisplayHeldBlockTask, info, TimeSpan.FromMilliseconds(i * 125));
                }
            }
            Drop leftovers = null;
            if (drop.items != null) {
                foreach (Item item in drop.items) {
                    if (!GetItem(item)) {
                        if (leftovers == null) {
                            leftovers = new Drop(item);
                        } else {
                            leftovers.items.Add(item);
                        }
                    }
                }
                UpdateItemDisplay();
            }
            return leftovers;

        }

        public void SetAmount(BlockID clientBlockID, int amount, bool displayChange = true, bool showToNormalChat = false) {
            //TODO threadsafe

            blocks[clientBlockID] += amount;


            if (displayChange) {
                NasBlock nb = NasBlock.Get(clientBlockID);
                DisplayHeldBlock(nb, amount, showToNormalChat);
            }

            if (blocks[clientBlockID] > 0) {
                //more than zero? unhide the block
                UnhideBlock(clientBlockID);
                return;
            } else {
                //0 or less? hide the block
                HideBlock(clientBlockID);
            }

        }
        public int GetAmount(BlockID clientBlockID) {
            //TODO threadsafe
            return blocks[clientBlockID];
        }

        [JsonIgnore] public CpeMessageType whereHeldBlockIsDisplayed = CpeMessageType.BottomRight3;
        public void DisplayHeldBlock(NasBlock nasBlock, int amountChanged = 0, bool showToNormalChat = false) {

            string display = DisplayedBlockString(nasBlock);
            if (amountChanged > 0) {
                display = "%a+" + amountChanged + " %f" + display;
            }
            if (amountChanged < 0) {
                display = "%c" + amountChanged + " %f" + display;
            }
            if (showToNormalChat) {
                p.Message(display);
            }
            p.SendCpeMessage(whereHeldBlockIsDisplayed, display);
        }
        string DisplayedBlockString(NasBlock nasBlock) {
            if (nasBlock.parentID == 0) {
                return "┤";
            }
            int amount = GetAmount(nasBlock.parentID);
            string hand = amount <= 0 ? "┤" : "╕¼";

            return "[" + amount + "] " + nasBlock.GetName(p) + " " + hand;
        }
        private class DisplayInfo {
            public Inventory inv;
            public NasBlock nasBlock;
            public int amountChanged;
            public bool showToNormalChat;
        }
        static void DisplayHeldBlockTask(SchedulerTask task) {
            DisplayInfo info = (DisplayInfo)(task.State);
            info.inv.DisplayHeldBlock(info.nasBlock, info.amountChanged, info.showToNormalChat);
        }

        void HideBlock(BlockID clientBlockID) {
            p.Send(Packet.BlockPermission(clientBlockID, false, false, true));
            p.Send(Packet.SetInventoryOrder(clientBlockID, 0, true));

            NasBlock nasBlock = NasBlock.blocks[clientBlockID];
            if (nasBlock.childIDs != null) {
                foreach (BlockID childID in nasBlock.childIDs) {
                    p.Send(Packet.BlockPermission(childID, false, false, true));
                    p.Send(Packet.SetInventoryOrder(childID, 0, true));
                }
            }
        }
        void UnhideBlock(BlockID clientBlockID) {
            BlockDefinition def = BlockDefinition.GlobalDefs[Block.FromRaw(clientBlockID)];
            if (def == null && clientBlockID < Block.CPE_COUNT) { def = DefaultSet.MakeCustomBlock(Block.FromRaw(clientBlockID)); }
            if (def == null) { return; }

            p.Send(Packet.BlockPermission(clientBlockID, true, false, true));
            p.Send(Packet.SetInventoryOrder(clientBlockID, (def.InventoryOrder == -1) ? clientBlockID : (ushort)def.InventoryOrder, true));

            NasBlock nasBlock = NasBlock.blocks[clientBlockID];
            if (nasBlock.childIDs != null) {
                foreach (BlockID childID in nasBlock.childIDs) {
                    def = BlockDefinition.GlobalDefs[Block.FromRaw(childID)];
                    if (def == null && childID < Block.CPE_COUNT) { def = DefaultSet.MakeCustomBlock(Block.FromRaw(childID)); }
                    if (def == null) { continue; }
                    p.Send(Packet.BlockPermission(childID, true, false, true));
                    p.Send(Packet.SetInventoryOrder(childID, (def.InventoryOrder == -1) ? childID : (ushort)def.InventoryOrder, true));
                }
            }
        }

    }

}
