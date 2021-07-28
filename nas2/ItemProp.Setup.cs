using System;

namespace NotAwesomeSurvival {

    public partial class ItemProp {

        public static void Setup() {
            ItemProp fist = new ItemProp("Fist||¬", NasBlock.Material.None, 0, 0);
            fist.baseHP = Int32.MaxValue;
            Item.Fist = new Item("Fist");
            
            ItemProp key = new ItemProp("Key|f|σ", NasBlock.Material.None, 0, 0);
            key.baseHP = Int32.MaxValue;

            ItemProp woodPick = new ItemProp("Wood Pickaxe|s|ß", NasBlock.Material.Stone, 0.0f, 1);
            woodPick.baseHP = 4;

            ItemProp stonePick = new ItemProp("Stone Pickaxe|7|ß", NasBlock.Material.Stone, 0.75f, 1);
            ItemProp stoneShovel = new ItemProp("Stone Shovel|7|Γ", NasBlock.Material.Earth, 0.50f, 1);
            ItemProp stoneAxe = new ItemProp("Stone Axe|7|π", NasBlock.Material.Wood, 0.60f, 1);
            stoneAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp stoneSword = new ItemProp("Stone Sword|7|α", NasBlock.Material.Leaves, 0.50f, 1);
            stoneSword.damage = 2.5f;

            const int ironBaseHP = baseHPconst * 8;
            ItemProp ironPick = new ItemProp("Iron Pickaxe|f|ß", NasBlock.Material.Stone, 0.85f, 2);
            ItemProp ironShovel = new ItemProp("Iron Shovel|f|Γ", NasBlock.Material.Earth, 0.60f, 2);
            ItemProp ironAxe = new ItemProp("Iron Axe|f|π", NasBlock.Material.Wood, 0.75f, 2);
            ironAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp ironSword = new ItemProp("Iron Sword|f|α", NasBlock.Material.Leaves, 0.75f, 2);
            ironSword.damage = 3.4f;
            ironPick.baseHP = ironBaseHP;
            ironShovel.baseHP = ironBaseHP;
            ironAxe.baseHP = ironBaseHP;
            ironSword.baseHP = ironBaseHP;

            const int goldBaseHP = baseHPconst * 64;
            ItemProp goldPick = new ItemProp("Gold Pickaxe|6|ß", NasBlock.Material.Stone, 0.90f, 3);
            ItemProp goldShovel = new ItemProp("Gold Shovel|6|Γ", NasBlock.Material.Earth, 0.85f, 3);
            ItemProp goldAxe = new ItemProp("Gold Axe|6|π", NasBlock.Material.Wood, 0.90f, 3);
            goldAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp goldSword = new ItemProp("Gold Sword|6|α", NasBlock.Material.Leaves, 0.85f, 3);
            goldSword.damage = 5f;
            goldPick.baseHP = goldBaseHP;
            goldShovel.baseHP = goldBaseHP;
            goldAxe.baseHP = goldBaseHP;
            goldSword.baseHP = goldBaseHP;

            const int diamondBaseHP = baseHPconst * 128;
            ItemProp diamondPick = new ItemProp("Diamond Pickaxe|b|ß", NasBlock.Material.Stone, 0.95f, 3);
            ItemProp diamondShovel = new ItemProp("Diamond Shovel|b|Γ", NasBlock.Material.Earth, 1f, 3);
            ItemProp diamondAxe = new ItemProp("Diamond Axe|b|π", NasBlock.Material.Wood, 0.95f, 3);
            diamondAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp diamondSword = new ItemProp("Diamond Sword|b|α", NasBlock.Material.Leaves, 1f, 3);
            diamondSword.damage = 10f;
            diamondPick.baseHP = diamondBaseHP;
            diamondShovel.baseHP = diamondBaseHP;
            diamondAxe.baseHP = diamondBaseHP;
            diamondSword.baseHP = diamondBaseHP;

        }
    }

}
