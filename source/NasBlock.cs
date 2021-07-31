using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Maths;
using BlockID = System.UInt16;
using NasBlockAction = System.Action<NotAwesomeSurvival.NasLevel, int, int, int>;

using NasBlockInteraction =
    System.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
    
using NasBlockExistAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
    
using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
        public static NasBlock[] blocks = new NasBlock[Block.MaxRaw + 1];
        public static NasBlock[] blocksIndexedByServerBlockID;
        
        public static NasBlock Default;
        public static int[] DefaultDurabilities = new int[(int)Material.Count];

        public static NasBlock Get(BlockID clientBlockID) {
            return (NasBlock.blocks[clientBlockID] == null) ?
                NasBlock.Default :
                NasBlock.blocks[clientBlockID];
        }
        /// <summary>
        /// Leave id arg blank to use parent's name
        /// </summary>
        public string GetName(Player p, BlockID id = BlockID.MaxValue) {
            if (id == BlockID.MaxValue) { id = parentID; }
            return GetBlockName(p, Block.FromRaw(id)).Split('-')[0];
        }
        public static string GetBlockName(Player p, BlockID block) {
            if (Block.IsPhysicsType(block)) return "Physics block";
            
            BlockDefinition def = null;
            if (!p.IsSuper) {
                def = p.level.GetBlockDef(block);
            } else {
                def = BlockDefinition.GlobalDefs[block];
            }
            if (def != null) { return def.Name; }
            
            return "Unknown";
        }

        static Drop DefaultDropHandler(BlockID id) {
            return new Drop(id);
        }

        //value is default durability, which is considered in terms of how many "hits" it takes to break
        public enum Material {
            None,
            Gas,
            Stone,
            Earth,
            Wood,
            Plant,
            Leaves,
            Organic,
            Glass,
            Metal,
            Liquid,
            Lava,
            Count
        }

        public BlockID selfID;
        public BlockID parentID;
        public List<BlockID> childIDs = null;
        public Material material;
        public int tierOfToolNeededToBreak;
        public Type type;
        public int durability;
        public float damageDoneToTool;
        public Func<BlockID, Drop> dropHandler;
        public int resourceCost;
        public Crafting.Station station;
        public Container container;
        
        public bool collides = true;
        public AABB bounds;
        public float fallDamageMultiplier = -1;
        
        public float disturbDelayMax = 0f;
        public float disturbDelayMin = 0f;
        public NasBlockAction disturbedAction = null;
        public NasBlockInteraction interaction = null;
        public NasBlockExistAction existAction = null;
        public NasBlockCollideAction collideAction = null;//DefaultCollideAction();

        public NasBlock(BlockID id, Material mat) {
            selfID = id;
            parentID = id;
            material = mat;
            tierOfToolNeededToBreak = 0;
            durability = DefaultDurabilities[(int)mat];
            damageDoneToTool = 1f;
            if (material == Material.Plant || material == Material.Leaves) { damageDoneToTool = 0; }
            dropHandler = DefaultDropHandler;
            resourceCost = 1;
            station = null;
            
        }
        public NasBlock(BlockID id, Material mat, int dur, int tierOfToolNeededToBreak = 0) : this(id, mat) {
            durability = dur;
            this.tierOfToolNeededToBreak = tierOfToolNeededToBreak;
        }
        public NasBlock(BlockID id, NasBlock parent) {
            selfID = id;
            if (blocks[parent.parentID].childIDs == null) {
                blocks[parent.parentID].childIDs = new List<BlockID>();
            }
            blocks[parent.parentID].childIDs.Add(id);

            parentID = parent.parentID;
            material = parent.material;
            tierOfToolNeededToBreak = parent.tierOfToolNeededToBreak;
            durability = parent.durability;
            damageDoneToTool = parent.damageDoneToTool;
            dropHandler = parent.dropHandler;
            resourceCost = parent.resourceCost;
            if (parent.station != null) {
                station = new Crafting.Station(parent.station);
            }
            if (parent.container != null) {
                container = new Container(parent.container);
            }
            if (parent.disturbedAction != null) {
                this.disturbedAction = parent.disturbedAction;
            }
            if (parent.interaction != null) {
                this.interaction = parent.interaction;
            }
            if (parent.existAction != null) {
                this.existAction = parent.existAction;
            }
        }
    }

}
