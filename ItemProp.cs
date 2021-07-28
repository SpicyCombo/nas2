using System.Collections.Generic;

namespace NotAwesomeSurvival {

    public partial class ItemProp {

        public string name;
        public string color;
        public string character;

        public List<NasBlock.Material> materialsEffectiveAgainst;
        public int tier;
        public float percentageOfTimeSaved;
        public const int baseHPconst = 200;
        public float baseHP;
        public float damage;

        public static Dictionary<string, ItemProp> props = new Dictionary<string, ItemProp>();

        public ItemProp(string description, NasBlock.Material effectiveAgainst = NasBlock.Material.None, float percentageOfTimeSaved = 0, int tier = 1) {
            string[] descriptionBits = description.Split('|');
            this.name = descriptionBits[0];
            this.color = descriptionBits[1];
            this.character = descriptionBits[2];

            if (effectiveAgainst != NasBlock.Material.None) {
                this.materialsEffectiveAgainst = new List<NasBlock.Material>();
                this.materialsEffectiveAgainst.Add(effectiveAgainst);
            } else {
                this.materialsEffectiveAgainst = null;
            }
            //tier 0 is fists
            this.tier = tier;
            this.percentageOfTimeSaved = percentageOfTimeSaved;
            this.baseHP = baseHPconst;
            this.damage = 1;
            props.Add(this.name, this);
        }
    }

}
