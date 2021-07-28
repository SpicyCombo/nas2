using System.Collections.Generic;
using BlockID = System.UInt16;

namespace NotAwesomeSurvival {

    //Stores information about a drop (from breaking a block or from a mob dying, or in a chest)
    public class Drop {
        public List<BlockStack> blockStacks = null;
        public List<Item> items = null;
        public Drop() {

        }
        public Drop(Drop parent) {
            if (parent.blockStacks != null) {
                this.blockStacks = new List<BlockStack>();
                foreach (BlockStack bs in parent.blockStacks) {
                    BlockStack bsClone = new BlockStack(bs.ID, bs.amount);
                    this.blockStacks.Add(bsClone);
                }
            }
            if (parent.items != null) {
                this.items = new List<Item>();
                foreach (Item item in parent.items) {
                    Item itemClone = new Item(item.name);
                    this.items.Add(itemClone);
                }
            }
        }
        public Drop(BlockID clientBlockID, int amount = 1) {
            BlockStack bs = new BlockStack(clientBlockID, amount);
            blockStacks = new List<BlockStack>();
            blockStacks.Add(bs);
        }
        public Drop(Item item) {
            items = new List<Item>();
            items.Add(item);
        }
        public Drop(Inventory inv) {
            blockStacks = new List<BlockStack>();
            for (int i = 0; i < inv.blocks.Length; i++) {
                if (inv.blocks[i] == 0) { continue; }
                blockStacks.Add(new BlockStack((ushort)i, inv.blocks[i]));
            }
            if (blockStacks.Count == 0) { blockStacks = null; }
            
            items = new List<Item>();
            foreach (Item item in inv.items) {
                if (item == null) { continue; }
                items.Add(item);
            }
            if (items.Count == 0) { items = null; }
        }

    }
    public class BlockStack {
        public int amount;
        public BlockID ID;
        public BlockStack(BlockID ID, int amount = 1) {
            this.ID = ID; this.amount = amount;
        }
    }

}
