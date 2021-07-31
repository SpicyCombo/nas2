using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Blocks;
using BlockID = System.UInt16;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
        static Random r = new Random();
        public static void Setup() {
            DefaultDurabilities[(int)Material.None] = 1;
            DefaultDurabilities[(int)Material.Gas] = 0;
            DefaultDurabilities[(int)Material.Stone] = 16;
            DefaultDurabilities[(int)Material.Earth] = 5;
            DefaultDurabilities[(int)Material.Wood] = 8;
            DefaultDurabilities[(int)Material.Plant] = 1;
            DefaultDurabilities[(int)Material.Leaves] = 3;
            DefaultDurabilities[(int)Material.Organic] = 5;
            DefaultDurabilities[(int)Material.Glass] = 3;
            DefaultDurabilities[(int)Material.Metal] = 32;

            Default = new NasBlock(0, Material.Earth);
            Default.collideAction = AirCollideAction();
            
            const float fallSpeed = 0.3f;

            BlockID i;
            
            i = 8; //active water
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = 1f;
            blocks[i].disturbDelayMax = 5f;
            blocks[i].disturbedAction = FloodAction(waterSet);
            blocks[i].collideAction = LiquidCollideAction();
            
            
            
            
            
            const float waterDisturbDelayMin = 0.5f;
            const float waterDisturbDelayMax = 0.5f;
            
            i = 643; //Water barrel
            blocks[i] = new NasBlock(i, Material.Wood, DefaultDurabilities[(int)Material.Wood]*2);
            blocks[i].childIDs = new List<BlockID>();
            blocks[i].childIDs.Add(9);
            
            i = 9; //still water
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].existAction = WaterExistAction();
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 1);
            blocks[i].collideAction = LiquidCollideAction();
            blocks[i].parentID = 643;
            

            
            i = 632; //water flows
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 3);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 4);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 5);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 6);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 7);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 8);
            blocks[i].collideAction = LiquidCollideAction();
            i++;
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = waterDisturbDelayMin;
            blocks[i].disturbDelayMax = waterDisturbDelayMax;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 9);
            blocks[i].collideAction = LiquidCollideAction();
            i = 639; //waterfall
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = fallSpeed;
            blocks[i].disturbDelayMax = fallSpeed;
            blocks[i].disturbedAction = LimitedFloodAction(waterSet, 2);
            blocks[i].collideAction = LiquidCollideAction();
            
            i = 10; //Active lava
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].disturbDelayMin = 1f;
            blocks[i].disturbDelayMax = 5f;
            blocks[i].disturbedAction = FloodAction(new BlockID[] {Block.Lava});
            blocks[i].collideAction = LavaCollideAction();
            
            i = 11; //Lava
            blocks[i] = new NasBlock(i, Material.Liquid, Int32.MaxValue);
            blocks[i].collideAction = LavaCollideAction();
            
            
            
            i = 1; //Stone
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 596; //Stone slab
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i, blocks[596]);
            i = 598; //Stone wall
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[598]);
            blocks[i] = new NasBlock(i++, blocks[598]);
            blocks[i] = new NasBlock(i++, blocks[598]);
            i = 70; //Stone stair (lower)
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[70]);
            blocks[i] = new NasBlock(i++, blocks[70]);
            blocks[i] = new NasBlock(i++, blocks[70]);
            i = 579; //Stone stair (upper)
            blocks[i] = new NasBlock(i++, blocks[70]);
            blocks[i] = new NasBlock(i++, blocks[70]);
            blocks[i] = new NasBlock(i++, blocks[70]);
            blocks[i] = new NasBlock(i++, blocks[70]);

            i = 162; //Cobblestone
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 163; //Cobblestone-U (next is D)
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i, blocks[163]);
            
            const int stonebrickDurMulti = 2;
            i = 64; //Marker
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone]*stonebrickDurMulti, 1);
            i = 65; //Stone brick
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone]*stonebrickDurMulti, 1);
            i = 86; //Stone brick slab
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone]*stonebrickDurMulti, 1);
            blocks[i] = new NasBlock(i++, blocks[86]);
            i = 75; //Stone pole
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[75]);
            blocks[i] = new NasBlock(i++, blocks[75]);
            i = 278; //Stone brick wall-N
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone]*stonebrickDurMulti, 1);
            blocks[i] = new NasBlock(i++, blocks[278]);
            blocks[i] = new NasBlock(i++, blocks[278]);
            blocks[i] = new NasBlock(i++, blocks[278]);
            i = 477; //Lined stone
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone]*stonebrickDurMulti, 1);
            
            i = 211; //Thin pole
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[211]);
            blocks[i] = new NasBlock(i++, blocks[211]);
            
            i = 214; //Boulder
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone]/2, 0);
            i = 194; //Nub
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone]/2, 0);
            
            
            const float grassDelayMin = 10;
            const float grassDelayMax = 60;
            i = 2; //Grass
            blocks[i] = new NasBlock(i, Material.Earth);
            blocks[i].disturbDelayMin = grassDelayMin;
            blocks[i].disturbDelayMax = grassDelayMax;
            blocks[i].disturbedAction = GrassBlockAction(Block.Grass, Block.Dirt);
            blocks[i].dropHandler = (dropID) => {
                Drop grassDrop = new Drop();
                grassDrop.blockStacks = new List<BlockStack>();
                grassDrop.blockStacks.Add(new BlockStack(3, 1));
                return grassDrop;
            };
            i = 129; //Wet grass
            blocks[i] = new NasBlock(i, Material.Earth);
            blocks[i].disturbDelayMin = grassDelayMin;
            blocks[i].disturbDelayMax = grassDelayMax;
            blocks[i].disturbedAction = GrassBlockAction(Block.Extended|129, Block.Dirt);
            blocks[i].dropHandler = (dropID) => {
                Drop grassDrop = new Drop();
                grassDrop.blockStacks = new List<BlockStack>();
                grassDrop.blockStacks.Add(new BlockStack(3, 1));
                return grassDrop;
            };
            
            

            i = 3; //Dirt
            blocks[i] = new NasBlock(i, Material.Earth);
            blocks[i].disturbDelayMin = grassDelayMin;
            blocks[i].disturbDelayMax = grassDelayMax;
            blocks[i].disturbedAction = DirtBlockAction(grassSet, Block.Dirt);

            i = 4; //Cobblebrick
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 50; //Cobble brick-D
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 59; //Cobble brick-U
            blocks[i] = new NasBlock(i, blocks[50]);
            i = 133; //Cobble brick wall
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i, blocks[133]);

            i = 5; //Wood
            blocks[i] = new NasBlock(i, Material.Wood);
            i = 56; //Wood slab-U
            blocks[i] = new NasBlock(i, Material.Wood);
            i = 57; //Wood slab-D
            blocks[i] = new NasBlock(i, blocks[56]);
            i = 182; //Wood wall
            blocks[i] = new NasBlock(i++, Material.Wood);
            blocks[i] = new NasBlock(i++, blocks[182]);
            blocks[i] = new NasBlock(i++, blocks[182]);
            blocks[i] = new NasBlock(i++, blocks[182]);
            i = 66; //Wood stair (lower)
            blocks[i] = new NasBlock(i++, Material.Wood);
            blocks[i] = new NasBlock(i++, blocks[66]);
            blocks[i] = new NasBlock(i++, blocks[66]);
            blocks[i] = new NasBlock(i++, blocks[66]);
            i = 567; //Wood stair (upper)
            blocks[i] = new NasBlock(i++, blocks[66]);
            blocks[i] = new NasBlock(i++, blocks[66]);
            blocks[i] = new NasBlock(i++, blocks[66]);
            blocks[i] = new NasBlock(i++, blocks[66]);
            i = 78; //Wood pole
            blocks[i] = new NasBlock(i++, Material.Wood);
            blocks[i] = new NasBlock(i++, blocks[78]);
            blocks[i] = new NasBlock(i++, blocks[78]);
            i = 94; //Fence (wood)
            blocks[i] = new NasBlock(i++, Material.Wood);
            blocks[i] = new NasBlock(i, blocks[94]);
            
            i = 168; //Wood board
            blocks[i] = new NasBlock(i++, Material.Wood);
            i = 524; //Board (sideways)
            blocks[i] = new NasBlock(i++, Material.Wood);
            blocks[i] = new NasBlock(i++, blocks[524]);
            blocks[i] = new NasBlock(i++, blocks[524]);
            blocks[i] = new NasBlock(i++, blocks[524]);
            
            

            i = 14; //Gnarly (Log)
            blocks[i] = new NasBlock(i, Material.Wood);
            i = 17; //Log-UD
            blocks[i] = new NasBlock(i, Material.Wood);
            i = 15; //Log-WE
            blocks[i] = new NasBlock(i, blocks[17]);
            i = 16; //Log-NS
            blocks[i] = new NasBlock(i, blocks[17]);

            const float treeDelayMin = 30f;
            const float treeDelayMax = 60f;
            i = 6; //Sapling
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbDelayMin = treeDelayMin;
            blocks[i].disturbDelayMax = treeDelayMax;
            blocks[i].disturbedAction = OakSaplingAction();

            i = 7; //Bedrock
            blocks[i] = new NasBlock(i, Material.Stone, int.MaxValue);

            i = 12; //Sand
            blocks[i] = new NasBlock(i, Material.Earth, 3);
            blocks[i].disturbDelayMin = fallSpeed;
            blocks[i].disturbDelayMax = fallSpeed;
            blocks[i].disturbedAction = FallingBlockAction(Block.Sand);

            i = 13; //Gravel
            blocks[i] = new NasBlock(i, Material.Earth);
            blocks[i].disturbDelayMin = fallSpeed;
            blocks[i].disturbDelayMax = fallSpeed;
            blocks[i].disturbedAction = FallingBlockAction(Block.Gravel);

            const float leafShrivelDelayMin = 1f;
            const float leafShrivelDelayMax = 1.5f;
            i = 18; //Leaves
            blocks[i] = new NasBlock(i, Material.Leaves);
            blocks[i].disturbedAction = LeafBlockAction(logSet, Block.Leaves);
            blocks[i].disturbDelayMin = leafShrivelDelayMin;
            blocks[i].disturbDelayMax = leafShrivelDelayMax;
            blocks[i].dropHandler = (dropID) => {
                Drop drop = new Drop(18, 1);
                
                int rand = r.Next(0, 8);
                if (rand == 0) { //16 in 128 chance (1 in 8 chance) of sapling
                    drop.blockStacks.Add(new BlockStack(6, 1));
                } else {
                    drop = new Drop(18, 1);
                }
                return drop;
            };
            i = 105; //Leaves slab
            blocks[i] = new NasBlock(i, Material.Leaves);
            
            
            i = 19; //Dark leaves
            blocks[i] = new NasBlock(i, Material.Leaves);

            i = 20; //Glass
            blocks[i] = new NasBlock(i, Material.Glass);
            i = 136; //Glass pane
            blocks[i] = new NasBlock(i++, Material.Glass);
            blocks[i] = new NasBlock(i, blocks[136]);
            
            i = 203; //Old glass
            blocks[i] = new NasBlock(i, Material.Glass);
            i = 209; //Old pane
            blocks[i] = new NasBlock(i++, Material.Glass);
            blocks[i] = new NasBlock(i, blocks[209]);
            
            i = 471; //New glass
            blocks[i] = new NasBlock(i, Material.Glass);
            i = 472; //New pane
            blocks[i] = new NasBlock(i++, Material.Glass);
            blocks[i] = new NasBlock(i, blocks[472]);
            

            i = 37; //Dandelion
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = GenericPlantAction();

            i = 38; //Rose
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = GenericPlantAction();
            i = 39; //Dead shrub
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = NeedsSupportAction();
            
            i = 40; //Tall grass
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = GenericPlantAction();
            i = 130; //Wet tall grass
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].dropHandler = (dropID) => {
                Drop wheatDrop = new Drop(644, 1);
                return (r.Next(0, 16) == 0) ? wheatDrop : null;
            };
            blocks[i].disturbedAction = GenericPlantAction();

            i = 41; //Gold
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 2);
            i = 42; //Iron
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            i = 631; //Diamond
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 3);
            i = 649; //Emerald
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 3);
            
            i = 148; //Old iron
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            i = 149; //Old iron slab
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            blocks[i] = new NasBlock(i, blocks[149]);
            i = 294; //Old iron wall
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            blocks[i] = new NasBlock(i++, blocks[294]);
            blocks[i] = new NasBlock(i++, blocks[294]);
            blocks[i] = new NasBlock(i, blocks[294]);
            
            i = 159; //Iron fence-WE
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            blocks[i].collideAction = AirCollideAction();
            i++;
            blocks[i] = new NasBlock(i, blocks[159]);
            i = 161; //Iron cage
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Metal], 1);
            blocks[i].collideAction = AirCollideAction();
            

            i = 44; //Concrete slab-D
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] / 2, 1);
            blocks[i].damageDoneToTool = 0.5f;
            i = 58; //Concrete slab-U
            blocks[i] = new NasBlock(i, blocks[44]);
            i = 43; //Double Concrete slab
            blocks[i] = new NasBlock(i, blocks[44]);
            blocks[i].durability = DefaultDurabilities[(int)Material.Stone];
            blocks[i].dropHandler = (dropID) => { return new Drop(dropID, 2); };
            blocks[i].resourceCost = 2;
            blocks[i].damageDoneToTool = 1f;
            i = 45; //Concrete block
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 282; //Concrete wall
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[282]);
            blocks[i] = new NasBlock(i++, blocks[282]);
            blocks[i] = new NasBlock(i++, blocks[282]);
            i = 549; //Concrete bricks[sic]
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 135; //Stone plate
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] / 2, 1);
            i = 270; //Concrete stairs
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[270]);
            blocks[i] = new NasBlock(i++, blocks[270]);
            blocks[i] = new NasBlock(i, blocks[270]);
            i = 587; //upper
            blocks[i] = new NasBlock(i++, blocks[270]);
            blocks[i] = new NasBlock(i++, blocks[270]);
            blocks[i] = new NasBlock(i++, blocks[270]);
            blocks[i] = new NasBlock(i, blocks[270]);
            i = 480; //Concrete corner
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i++, blocks[480]);
            blocks[i] = new NasBlock(i++, blocks[480]);
            blocks[i] = new NasBlock(i, blocks[480]);


            i = 48; //Obsidian slab-D
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 298; //Obsidian slab-U
            blocks[i] = new NasBlock(i, blocks[48]);
            i = 49; //Obsidian
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);

            i = 51; //Rope
            blocks[i] = new NasBlock(i, Material.Wood);

            i = 52; //Sandstone
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            i = 299; //Sandstone slab
            blocks[i] = new NasBlock(i++, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i] = new NasBlock(i, blocks[299]);

            i = 53; //Snow (layer)
            blocks[i] = new NasBlock(i, Material.Earth);

            i = 54; //Fire
            blocks[i] = new NasBlock(i, Material.None);

            i = 55; //Dark door
            blocks[i] = new NasBlock(i, Material.Wood);


            i = 60; //Ice
            blocks[i] = new NasBlock(i, Material.Stone, 8);

            i = 61; //Marble
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);

            i = 62; //Lamp
            blocks[i] = new NasBlock(i, Material.Glass);

            i = 63; //Pillar-UD
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);



            i = 74; //Dark pillar
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);


            i = 104; //Dry leaves
            blocks[i] = new NasBlock(i, Material.Leaves);

            
            //Drawers
            i = 198;
            blocks[i] = new NasBlock(i, blocks[17]);
            blocks[i].station = new Crafting.Station();
            blocks[i].station.name = "Crafting Station";
            blocks[i].station.type = Crafting.Station.Type.Normal;
            blocks[i].station.ori = Crafting.Station.Orientation.NS;
            blocks[i].existAction = CraftingExistAction();
            blocks[i].interaction = CraftingInteraction();
            i = 199;
            blocks[i] = new NasBlock(i, blocks[198]);
            blocks[i].station.ori = Crafting.Station.Orientation.WE;

            //Furnace
            i = 625;
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);
            blocks[i].station = new Crafting.Station();
            blocks[i].station.name = "Smelting Station";
            blocks[i].station.type = Crafting.Station.Type.Furnace;
            blocks[i].station.ori = Crafting.Station.Orientation.WE;
            blocks[i].existAction = CraftingExistAction();
            blocks[i].interaction = CraftingInteraction();
            i = 626;
            blocks[i] = new NasBlock(i, blocks[625]);
            blocks[i].station.ori = Crafting.Station.Orientation.NS;

            i = 239; //hotcoals
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 1);

            i = 204; //Monitor-S
            blocks[i] = new NasBlock(i++, Material.Metal, 3);
            blocks[i] = new NasBlock(i++, blocks[204]);
            blocks[i] = new NasBlock(i++, blocks[204]);
            blocks[i] = new NasBlock(i, blocks[204]);

            
            
            i = 142; //Crate
            blocks[i] = new NasBlock(i, Material.Wood, DefaultDurabilities[(int)Material.Wood]*2);
            blocks[i].interaction = CrateInteraction();
            
            i = 143; //Barrel
            blocks[i] = new NasBlock(i, Material.Wood, DefaultDurabilities[(int)Material.Wood]*2);
            blocks[i].container = new Container();
            blocks[i].container.type = Container.Type.Barrel;
            blocks[i].existAction = ContainerExistAction();
            blocks[i].interaction = ContainerInteraction();
            i = 602; //Barrel (sideways)
            blocks[i] = new NasBlock(i++, blocks[143]);
            blocks[i] = new NasBlock(i, blocks[143]);
            
            
            i = 216; //Chest-S
            blocks[i] = new NasBlock(i, Material.Wood, DefaultDurabilities[(int)Material.Wood]*2);
            blocks[i].container = new Container();
            blocks[i].container.type = Container.Type.Chest;
            blocks[i].existAction = ContainerExistAction();
            blocks[i].interaction = ContainerInteraction();
            i++;
            blocks[i] = new NasBlock(i, blocks[216]);
            i++;
            blocks[i] = new NasBlock(i, blocks[216]);
            i++;
            blocks[i] = new NasBlock(i, blocks[216]);


            i = 220; //Ladder
            blocks[i] = new NasBlock(i, Material.Wood, DefaultDurabilities[(int)Material.Wood]);
            i++;
            blocks[i] = new NasBlock(i, blocks[220]);
            i++;
            blocks[i] = new NasBlock(i, blocks[220]);
            i++;
            blocks[i] = new NasBlock(i, blocks[220]);
            
            
            i = 647; //Gravestone
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone], 0);
            blocks[i].container = new Container();
            blocks[i].container.type = Container.Type.Gravestone;
            blocks[i].existAction = ContainerExistAction();
            blocks[i].interaction = ContainerInteraction();
            
            

            i = 627; //Coal ore
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] + 2, 1);
            i = 628; //Iron ore
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] + 4, 1);
            i = 629; //Gold ore
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] + 6, 2);
            i = 630; //Diamond ore
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] + 6, 3);
            i = 650; //Emerald ore
            blocks[i] = new NasBlock(i, Material.Stone, DefaultDurabilities[(int)Material.Stone] + 6, 3);
            
            
            //from 20 to 40 minutes (avg 30)
            const float wheatTotalSeconds = 20f * 60f;
            const float wheatMaxAddedSeconds = 20f * 60f;
            const float wheatGrowMin = (wheatTotalSeconds)/3f;
            const float wheatGrowMax = (wheatTotalSeconds + wheatMaxAddedSeconds)/3f;
            i = 644; //Wheat (baby)
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = CropAction(wheatSet, 0);
            blocks[i].disturbDelayMin = wheatGrowMin;
            blocks[i].disturbDelayMax = wheatGrowMax;
            blocks[i].dropHandler = CustomDrop(644, 1);
            i = 645;
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = CropAction(wheatSet, 1);
            blocks[i].disturbDelayMin = wheatGrowMin;
            blocks[i].disturbDelayMax = wheatGrowMax;
            blocks[i].dropHandler = CustomDrop(644, 1);
            i = 646;
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = CropAction(wheatSet, 2);
            blocks[i].disturbDelayMin = wheatGrowMin;
            blocks[i].disturbDelayMax = wheatGrowMax;
            blocks[i].dropHandler = CustomDrop(644, 1);
            i = 461; //Wheat (full grown)
            blocks[i] = new NasBlock(i, Material.Plant);
            blocks[i].disturbedAction = CropAction(wheatSet, 3);
            blocks[i].dropHandler = (dropID) => {
                if (r.Next(0, 2) == 0) {
                    return new Drop(145, 1);
                }
                return new Drop(644, r.Next(2, 5));
                
                //Drop drop = new Drop();
                //drop.blockStacks = new List<BlockStack>();
                //drop.blockStacks.Add(new BlockStack(644, r.Next(1, 4)));
                //drop.blockStacks.Add(new BlockStack(145, 1));
                //return drop;
            };
            
            i = 145;
            blocks[i] = new NasBlock(i, Material.Organic, 3);
            i = 622;
            blocks[i] = new NasBlock(i++, blocks[145]);
            blocks[i] = new NasBlock(i++, blocks[145]);
            
            
            const float breadRestore = 1f;
            i = 640; //Loaf of bread
            blocks[i] = new NasBlock(i, Material.Organic, 3);
            blocks[i].interaction = EatInteraction(breadSet, 0, breadRestore*2);
            i++;
            blocks[i] = new NasBlock(i, Material.Organic, 3);
            blocks[i].interaction = EatInteraction(breadSet, 1, breadRestore);
            i++;
            blocks[i] = new NasBlock(i, Material.Organic, 3);
            blocks[i].interaction = EatInteraction(breadSet, 2, breadRestore);
            
            i = 648; //Apple
            blocks[i] = new NasBlock(i, Material.Organic, 3);
            blocks[i].disturbDelayMin = fallSpeed;
            blocks[i].disturbDelayMax = fallSpeed;
            blocks[i].disturbedAction = FallingBlockAction(Block.FromRaw(i));
            blocks[i].interaction = EatInteraction(new BlockID[] { Block.FromRaw(648) } , 0, 1f);

        }
        static Func<BlockID, Drop> CustomDrop(BlockID clientBlockID, int amount) {
            return (dropID) => { return new Drop(clientBlockID, amount); };
        }
    } //class NasBlock

}
