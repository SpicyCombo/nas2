using System;
using System.Text;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Network;

namespace NotAwesomeSurvival {

    public partial class Inventory {

        public Item[] items = new Item[maxItems];
        public const int maxItems = 27;
        public const int itemBarLength = 9;
        public int selectedItemIndex = 0;
        [JsonIgnore] public ColorDesc[] selectorColors = DynamicColor.defaultColors;

        [JsonIgnore]
        public Item HeldItem {
            get { return items[selectedItemIndex] == null ? Item.Fist : items[selectedItemIndex]; }
        }

        public void SetupItems() {
            MoveBar(0, ref selectedItemIndex);
        }

        //returns false if the player doesn't have room for the item
        public bool GetItem(Item item) {
            if (items[selectedItemIndex] == null) {
                items[selectedItemIndex] = item;
                p.Message("You got {0}%S!", item.ColoredName);
                return true;
            }
            for (int i = 0; i < maxItems; i++) {
                if (items[i] == null) {
                    items[i] = item;
                    p.Message("You got {0}%S!", item.ColoredName);
                    return true;
                }
            }
            p.Message("You can't get {0}%S because your tool bag is full.", item.ColoredName);
            return false;
        }
        [JsonIgnore] public bool bagOpen = false;
        [JsonIgnore] private int slotToMoveTo = -1;
        public void ToggleBagOpen() {
            deleting = false;
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            bagOpen = !bagOpen;
            if (bagOpen) {
                p.Send(Packet.Motd(p, "-hax horspeed=0.000001"));
                whereHeldBlockIsDisplayed = CpeMessageType.Status2;
                np.whereHealthIsDisplayed = CpeMessageType.Status3;
            } else {
                p.SendMapMotd();
                p.SendCpeMessage(CpeMessageType.Status2, "");
                p.SendCpeMessage(CpeMessageType.Status3, "");
                whereHeldBlockIsDisplayed = CpeMessageType.BottomRight3;
                np.whereHealthIsDisplayed = CpeMessageType.BottomRight2;
            }
            if (slotToMoveTo != -1) {
                MoveBar(0, ref slotToMoveTo);
            } else {
                MoveBar(0, ref selectedItemIndex);
            }

            DisplayHeldBlock(np.heldNasBlock);
            np.DisplayHealth();
        }
        public void DoItemMove() {
            if (slotToMoveTo == -1) { BeginItemMove(); } else { FinalizeItemMove(); }
            UpdateItemDisplay();
        }
        private void BeginItemMove() {
            //p.Message("move item where?");
            slotToMoveTo = selectedItemIndex;
        }
        private void FinalizeItemMove() {
            Item gettingMoved = items[selectedItemIndex];
            Item to = items[slotToMoveTo];

            //swap around
            items[selectedItemIndex] = to;
            items[slotToMoveTo] = gettingMoved;

            //p.Message("moved item from slot {0} to slot {1}", itemBarSelection, slotToMoveTo);
            selectedItemIndex = slotToMoveTo;
            slotToMoveTo = -1;
        }
        public void MoveItemBarSelection(int direction) {
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            if (!np.hasBeenSpawned) {
                p.Message("%chasBeenSpawned is %cfalse%S, this shouldn't happen if you didn't just die.");
                p.Message("%bPlease report to Goodly what you were doing before this happened");
            }
            
            Item heldItemBeforeScrolled = HeldItem;

            deleting = false;
            if (slotToMoveTo != -1) {
                MoveBar(direction, ref slotToMoveTo);
                return;
            }
            MoveBar(direction, ref selectedItemIndex);

            if (heldItemBeforeScrolled != HeldItem) {
                //only reset breaking if they actually are holding a different item than before
                
                np.ResetBreaking();
                NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
                
            }

        }
        private void MoveBar(int direction, ref int selection) {
            int length = bagOpen ? maxItems : itemBarLength;
            if (bagOpen) {
                int offset = 0;
            thing:
                if (offset <= maxItems - itemBarLength) {

                    if (selection == offset + itemBarLength - 1 && selection + direction == offset + itemBarLength) {
                        direction -= itemBarLength;
                    } else if (selection == offset && selection + direction == offset - 1) {
                        direction += itemBarLength;
                    }
                    offset += itemBarLength;
                    goto thing;
                }
            }

            selection += direction;
            selection = selection % length;
            if (selection < 0) { selection += length; }
            UpdateItemDisplay();
        }

        public void UpdateItemDisplay() {
            selectorColors = HeldItem.healthColors;
            if (bagOpen) {
                DisplayItemBar(0, "%7↑ª", "%7ª↑", CpeMessageType.BottomRight3);
                DisplayItemBar(itemBarLength, "%7←¥", "%7₧→", CpeMessageType.BottomRight2);
                DisplayItemBar(itemBarLength * 2, "%7↓º", "%7º↓", CpeMessageType.BottomRight1);
                return;
            }
            DisplayItemBar();
        }
        private void DisplayItemBar(int offset = 0, string prefix = "%7←«", string suffix = "%7»→",
                                   CpeMessageType location = CpeMessageType.BottomRight1) {

            StringBuilder builder = new StringBuilder(prefix);

            for (int i = offset; i < itemBarLength + offset; i++) {
                bool moving = !(slotToMoveTo == -1);
                bool handsHere = i == slotToMoveTo;
                bool selectionHere = i == selectedItemIndex;
                bool selectionNext = moving ? i + 1 == slotToMoveTo : i + 1 == selectedItemIndex;
                int itemIndex = i;

                if (handsHere) { builder.Append("&h╣"); } else if (selectionHere && !moving) {
                    if (deleting) {
                        builder.Append("&h╙");
                    } else {
                        builder.Append("&hƒ");
                    }
                } else if (i == offset) { builder.Append("⌐"); }

                if (handsHere) {
                    itemIndex = selectedItemIndex;
                } else if (moving && !handsHere && selectionHere) {
                    itemIndex = slotToMoveTo;
                }

                Item item = items[itemIndex];

                if (item == null) { builder.Append("¬"); } else { builder.Append(item.ColoredIcon); }


                if (handsHere) { builder.Append("&h╕"); } else if (selectionHere && !moving) {
                    if (deleting) {
                        builder.Append("&h╙");
                    } else {
                        builder.Append("&h½");
                    }
                } else if (!selectionNext || i == itemBarLength + offset - 1) {
                    builder.Append("⌐");
                }
            }
            builder.Append(suffix);
            string final = builder.ToString();
            p.SendCpeMessage(location, final);

            //p.Message("Length of it is {0}", final.Length);
        }

        [JsonIgnore] private bool deleting = false;
        public void DeleteItem(bool confirmed = false) {
            //don't even fuck with deleting if they're moving items
            if (slotToMoveTo != -1) { return; }

            Item item = items[selectedItemIndex];
            if (item == null) { return; }

            if (deleting) {
                if (!confirmed) {
                    p.Message("Are you sure you want to delete {0}%S?", item.ColoredName);
                    p.Message("Press P to Put it in the trash.");
                    return;
                }
                p.Message("Deleted {0}.", item.name);
                items[selectedItemIndex] = null;
                deleting = false;
                UpdateItemDisplay();
                return;
            }
            if (confirmed) { return; }
            p.Message("Are you sure you want to delete {0}%S?", item.ColoredName);
            p.Message("Press P to Put it in the trash.");
            deleting = true;
            UpdateItemDisplay();
        }
        public void BreakItem(ref Item item) {

            for (int i = 0; i < maxItems; i++) {
                if (item == items[i]) {
                    p.Message("Your {0}%S broke!", item.ColoredName);
                    items[i] = null;
                    break;
                }
            }
        }


    } //class Inventory

}
