using BlockID = System.UInt16;

namespace NotAwesomeSurvival {

    public partial class Crafting {

        public static void Setup() {
            Recipe woodPickaxe = new Recipe(new Item("Wood Pickaxe"));
            //woodPickaxe.shapeless = true;
            //woodPickaxe.usesParentID = true;
            //woodPickaxe.stationType = Crafting.Station.Type.Furnace;
            woodPickaxe.pattern = new BlockID[,] {
                {  5,  5, 5 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };

            // stone tools
            Recipe stonePickaxe = new Recipe(new Item("Stone Pickaxe"));
            stonePickaxe.pattern = new BlockID[,] {
                {  1,  1, 1 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe stoneShovel = new Recipe(new Item("Stone Shovel"));
            stoneShovel.pattern = new BlockID[,] {
                {   1 },
                {  78 },
                {  78 }
            };
            Recipe stoneAxe = new Recipe(new Item("Stone Axe"));
            stoneAxe.pattern = new BlockID[,] {
                {  1,  1 },
                {  1, 78 },
                {  0, 78 }
            };

            Recipe stoneSword = new Recipe(new Item("Stone Sword"));
            stoneSword.pattern = new BlockID[,] {
                {  1 },
                {  1 },
                { 78 }
            };

            //iron tools
            Recipe ironPickaxe = new Recipe(new Item("Iron Pickaxe"));
            ironPickaxe.pattern = new BlockID[,] {
                { 42, 42,42 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe ironShovel = new Recipe(new Item("Iron Shovel"));
            ironShovel.pattern = new BlockID[,] {
                {  42 },
                {  78 },
                {  78 }
            };
            Recipe ironAxe = new Recipe(new Item("Iron Axe"));
            ironAxe.pattern = new BlockID[,] {
                { 42, 42 },
                { 42, 78 },
                {  0, 78 }
            };

            Recipe ironSword = new Recipe(new Item("Iron Sword"));
            ironSword.pattern = new BlockID[,] {
                { 42 },
                { 42 },
                { 78 }
            };

            //gold tools
            Recipe goldPickaxe = new Recipe(new Item("Gold Pickaxe"));
            goldPickaxe.pattern = new BlockID[,] {
                { 41, 41,41 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe goldShovel = new Recipe(new Item("Gold Shovel"));
            goldShovel.pattern = new BlockID[,] {
                {  41 },
                {  78 },
                {  78 }
            };
            Recipe goldAxe = new Recipe(new Item("Gold Axe"));
            goldAxe.pattern = new BlockID[,] {
                { 41, 41 },
                { 41, 78 },
                {  0, 78 }
            };

            Recipe goldSword = new Recipe(new Item("Gold Sword"));
            goldSword.pattern = new BlockID[,] {
                { 41 },
                { 41 },
                { 78 }
            };

            //diamond tools
            Recipe diamondPickaxe = new Recipe(new Item("Diamond Pickaxe"));
            diamondPickaxe.pattern = new BlockID[,] {
                { 631, 631,631 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe diamondShovel = new Recipe(new Item("Diamond Shovel"));
            diamondShovel.pattern = new BlockID[,] {
                {  631 },
                {  78 },
                {  78 }
            };
            Recipe diamondAxe = new Recipe(new Item("Diamond Axe"));
            diamondAxe.pattern = new BlockID[,] {
                { 631, 631 },
                { 631, 78 },
                {  0, 78 }
            };

            Recipe diamondSword = new Recipe(new Item("Diamond Sword"));
            diamondSword.pattern = new BlockID[,] {
                { 631 },
                { 631 },
                { 78 }
            };


            //wood stuff ------------------------------------------------------
            Recipe wood = new Recipe(5, 4);
            wood.usesParentID = true;
            wood.pattern = new BlockID[,] {
                {  17 }
            };
            Recipe woodSlab = new Recipe(56, 6);
            woodSlab.pattern = new BlockID[,] {
                {  5, 5, 5 }
            };
            Recipe woodWall = new Recipe(182, 6);
            woodWall.pattern = new BlockID[,] {
                {  5 },
                {  5 },
                {  5 }
            };
            Recipe woodStair = new Recipe(66, 6);
            woodStair.pattern = new BlockID[,] {
                {  5, 0, 0 },
                {  5, 5, 0 },
                {  5, 5, 5 }
            };

            Recipe Ladder = new Recipe(220, 4);
            Ladder.usesParentID = true;
            Ladder.pattern = new BlockID[,] {
                {  78, 0, 78 },
                {  78, 78, 78 },
                {  78, 0, 78 }
            };

            Recipe Rope = new Recipe(71, 2);
            Rope.pattern = new BlockID[,] {
                {  145 }
            };

            Recipe woodPole = new Recipe(78, 4);
            woodPole.pattern = new BlockID[,] {
                {  5 },
                {  5 }
            };

            Recipe fenceWE = new Recipe(94, 4);
            fenceWE.pattern = new BlockID[,] {
                {  78, 79, 78 },
                {  78, 79, 78 }
            };
            Recipe fenceNS = new Recipe(94, 4);
            fenceNS.pattern = new BlockID[,] {
                {  78, 80, 78 },
                {  78, 80, 78 }
            };

            Recipe darkDoor = new Recipe(55, 2);
            darkDoor.pattern = new BlockID[,] {
                { 17, 17 },
                { 17, 17 },
                { 17, 17 }
            };
            
            Recipe board = new Recipe(168, 6);
            board.usesParentID = true;
            board.pattern = new BlockID[,] {
                {  56, 56, 56 }
            };
            Recipe boardSideways = new Recipe(524, 6);
            boardSideways.usesParentID = true;
            boardSideways.pattern = new BlockID[,] {
                {  182 },
                {  182 },
                {  182 }
            };
            
            
            //chest
            Recipe chest = new Recipe(216, 1);
            chest.pattern = new BlockID[,] {
                {  5,  5,  5 },
                {  5, 148, 5 },
                {  5,  5,  5 }
            };
            
            Recipe barrel = new Recipe(143, 1);
            barrel.pattern = new BlockID[,] {
                { 150 },
                {  17 },
                { 149 }
            };
            
            Recipe crate = new Recipe(142, 1);
            crate.pattern = new BlockID[,] {
                { 5, 5 },
                { 5, 5 }
            };
            
            

            //stone stuff ------------------------------------------------------

            Recipe stoneSlab = new Recipe(596, 6);
            stoneSlab.pattern = new BlockID[,] {
                {  1, 1, 1 }
            };
            Recipe stoneWall = new Recipe(598, 6);
            stoneWall.pattern = new BlockID[,] {
                {  1 },
                {  1 },
                {  1 }
            };
            Recipe stoneStair = new Recipe(70, 6);
            stoneStair.pattern = new BlockID[,] {
                {  1, 0, 0 },
                {  1, 1, 0 },
                {  1, 1, 1 }
            };

            
            //stonebrick
            Recipe marker = new Recipe(64, 1);
            marker.pattern = new BlockID[,] {
                {  65, 65, 65 },
                {  65,  0, 65 },
                {  65, 65, 65 }
            };
            Recipe stoneBrick = new Recipe(65, 6);
            stoneBrick.pattern = new BlockID[,] {
                {  1, 1, 0 },
                {  0, 1, 1 },
                {  1, 1, 0 }
            };
            Recipe stoneBrickSlab = new Recipe(86, 6);
            stoneBrickSlab.pattern = new BlockID[,] {
                {  65, 65, 65 }
            };
            Recipe stoneBrickWall = new Recipe(278, 6);
            stoneBrickWall.pattern = new BlockID[,] {
                {  65 },
                {  65 },
                {  65 }
            };
            Recipe stonePole = new Recipe(75, 4);
            stonePole.pattern = new BlockID[,] {
                {  65 },
                {  65 }
            };
            Recipe thinPole = new Recipe(211, 4);
            thinPole.pattern = new BlockID[,] {
                {  75 },
                {  75 }
            };
            Recipe linedStone = new Recipe(477, 1);
            linedStone.pattern = new BlockID[,] {
                {  65, 65 },
                {  65, 65 }
            };
            

            Recipe boulder = new Recipe(214, 4);
            boulder.pattern = new BlockID[,] {
                {  1 }
            };
            Recipe nub = new Recipe(194, 4);
            nub.pattern = new BlockID[,] {
                {  214 }
            };
            
            
            
            
            Recipe cobbleBrick = new Recipe(4, 4);
            cobbleBrick.pattern = new BlockID[,] {
                {  162, 162 },
                {  162, 162 }
            };
            Recipe cobbleBrickSlab = new Recipe(50, 6);
            cobbleBrickSlab.pattern = new BlockID[,] {
                {  4, 4, 4 }
            };
            Recipe cobbleBrickWall = new Recipe(133, 6);
            cobbleBrickWall.pattern = new BlockID[,] {
                {  4, 4, 4 },
                {  4, 4, 4 }
            };



            Recipe cobblestone = new Recipe(162, 4);
            cobblestone.pattern = new BlockID[,] {
                {  1, 1 },
                {  1, 1 }
            };
            Recipe cobblestoneSlab = new Recipe(163, 6);
            cobblestoneSlab.pattern = new BlockID[,] {
                {  162, 162, 162 }
            };

            Recipe furnace = new Recipe(625, 1);
            furnace.usesParentID = true;
            furnace.pattern = new BlockID[,] {
                {  1,  1, 1 },
                {  1,  0, 1 },
                {  1,  1, 1 }
            };


            Recipe concreteBlock = new Recipe(45, 4);
            concreteBlock.pattern = new BlockID[,] {
                {  4, 4 },
                {  4, 4 }
            };
            Recipe concreteSlab = new Recipe(44, 6);
            concreteSlab.pattern = new BlockID[,] {
                {  45, 45, 45 }
            };

            Recipe sandstone = new Recipe(52, 4);
            sandstone.shapeless = true;
            sandstone.pattern = new BlockID[,] {
                { 12, 12 },
                { 12, 12 }
            };

            Recipe concreteWall = new Recipe(282, 6);
            concreteWall.pattern = new BlockID[,] {
                { 45 },
                { 45 },
                { 45 }
            };
            Recipe concreteBrick = new Recipe(549, 4);
            concreteBrick.pattern = new BlockID[,] {
                {  45, 45 },
                {  45, 45 }
            };
            Recipe stonePlate = new Recipe(135, 6);
            stonePlate.pattern = new BlockID[,] {
                {  44, 44, 44 }
            };
            //upside down slab recipe
            Recipe stonePlate2 = new Recipe(135, 6);
            stonePlate2.pattern = new BlockID[,] {
                {  58, 58, 58 }
            };
            Recipe concreteStair = new Recipe(270, 6);
            concreteStair.pattern = new BlockID[,] {
                {  45,  0,  0 },
                {  45, 45,  0 },
                {  45, 45, 45 }
            };
            Recipe concreteCorner = new Recipe(480, 4);
            concreteCorner.pattern = new BlockID[,] {
                { 45 },
                { 45 }
            };



            //ore stuff
            Recipe coalBlock = new Recipe(49, 1);
            coalBlock.shapeless = true;
            coalBlock.pattern = new BlockID[,] {
                {  627, 627 }
            };
            Recipe hotCoals = new Recipe(239, 1);
            hotCoals.shapeless = true;
            hotCoals.pattern = new BlockID[,] {
                {  49, 49 }
            };

            
            Recipe iron = new Recipe(42, 1);
            iron.stationType = Crafting.Station.Type.Furnace;
            iron.shapeless = true;
            iron.pattern = new BlockID[,] {
                {  628, 627, 627 },
                {  627, 627, 627 },
                {  627, 627, 627 },
            };
            //old iron
            Recipe oldIron = new Recipe(148, 1);
            oldIron.stationType = Crafting.Station.Type.Furnace;
            oldIron.shapeless = true;
            oldIron.pattern = new BlockID[,] {
                {  628, 627 }
            };
            Recipe oldIronSlab = new Recipe(149, 6);
            oldIronSlab.pattern = new BlockID[,] {
                {  148, 148, 148 }
            };
            Recipe oldIronWall = new Recipe(294, 6);
            oldIronWall.pattern = new BlockID[,] {
                { 148 },
                { 148 },
                { 148 }
            };
            
            Recipe key = new Recipe(new Item("Key"));
            key.usesParentID = true;
            key.pattern = new BlockID[,] {
                {  294, 149, 294 },
                {  149, 148,  0  },
                {  149, 148,  0  }
            };
            
            //i = 159; //Iron fence-WE
            Recipe ironFence = new Recipe(159, 12);
            ironFence.pattern = new BlockID[,] {
                {  148, 148, 148 },
                {  148, 148, 148 }
            };
            //i = 161; //Iron cage
            Recipe ironCage = new Recipe(161, 4);
            ironCage.usesParentID = true;
            ironCage.pattern = new BlockID[,] {
                {    0, 159,   0 },
                {  159,   0, 159 },
                {    0, 159,   0 }
            };
            
            
            Recipe gold = new Recipe(41, 1);
            gold.stationType = Crafting.Station.Type.Furnace;
            gold.shapeless = true;
            gold.pattern = new BlockID[,] {
                {  629, 49, 49 },
                {   49, 49, 49 },
                {   49, 49, 49 },
            };
            Recipe diamond = new Recipe(631, 1);
            diamond.stationType = Crafting.Station.Type.Furnace;
            diamond.shapeless = true;
            diamond.pattern = new BlockID[,] {
                {  630, 239, 239 },
                {  239, 239, 239 },
                {  239, 239, 239 },
            };

            //glass
            Recipe glass = new Recipe(20, 1);
            glass.stationType = Crafting.Station.Type.Furnace;
            glass.shapeless = true;
            glass.pattern = new BlockID[,] {
                { 12 }
            };

            Recipe lamp = new Recipe(62, 1);
            lamp.pattern = new BlockID[,] {
                {  57, 57, 57 },
                {  20, 49, 20 },
                {  56, 56, 56 }
            };

            Recipe glassPane = new Recipe(136, 6);
            glassPane.pattern = new BlockID[,] {
                {  20, 20, 20 },
                {  20, 20, 20 }
            };
            
            Recipe oldGlass = new Recipe(203, 1);
            oldGlass.pattern = new BlockID[,] {
                { 57 },
                { 20 }
            };
            Recipe oldGlassPane = new Recipe(209, 6);
            oldGlassPane.pattern = new BlockID[,] {
                {  203, 203, 203 },
                {  203, 203, 203 }
            };
            
            Recipe newGlass = new Recipe(471, 1);
            newGlass.pattern = new BlockID[,] {
                { 150 },
                {  20 }
            };
            Recipe newGlassPane = new Recipe(472, 6);
            newGlassPane.pattern = new BlockID[,] {
                {  471, 471, 471 },
                {  471, 471, 471 }
            };
            
            //bread
            Recipe bread = new Recipe(640, 1);
            bread.stationType = Crafting.Station.Type.Furnace;
            bread.shapeless = true;
            bread.usesParentID = true;
            bread.pattern = new BlockID[,] {
                { 145, 145, 145 }
            };
            
            
            Recipe leavesSlab = new Recipe(105, 6);
            leavesSlab.pattern = new BlockID[,] {
                {  18, 18, 18 }
            };
            
        }

    }

}
