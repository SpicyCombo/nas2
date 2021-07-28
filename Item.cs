using System;
using Newtonsoft.Json;
using MCGalaxy;

namespace NotAwesomeSurvival {

    public class Item {
        public static Item Fist;
        public string name;
        public float HP;

        [JsonIgnore] public ItemProp prop { get { return ItemProp.props[name]; } }

        public Item(string name) {
            ItemProp prop = ItemProp.props[name];
            this.name = prop.name;
            this.HP = prop.baseHP;
        }
        [JsonIgnore]
        public string ColoredName {
            get { return "&" + ItemProp.props[name].color + name; }
        }
        [JsonIgnore]
        public string ColoredIcon {
            get { return "&" + ItemProp.props[name].color + ItemProp.props[name].character; }
        }
        [JsonIgnore]
        public ColorDesc[] healthColors {
            get {
                if (HP == Int32.MaxValue) { return DynamicColor.defaultColors; }
                if (HP <= 1) { return DynamicColor.direHealthColors; }

                float healthPercent = HP / prop.baseHP;
                if (healthPercent > 0.5f) { return DynamicColor.fullHealthColors; }
                if (healthPercent > 0.25) { return DynamicColor.mediumHealthColors; }
                return DynamicColor.lowHealthColors;
            }
        }
        /// <summary>
        /// Call to take damage
        /// </summary>
        /// <param name="amount">the amount of damage to take. Breaking a block normally gives 1 damage</param>
        /// <returns>true if the item should break</returns>
        public bool TakeDamage(float amount = 1) {
            if (HP == Int32.MaxValue) { return false; }
            HP -= amount;
            if (HP <= 0) {
                return true;
            }
            return false;
        }

    }

}
