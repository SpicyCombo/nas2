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
using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
            
            
            public static NasBlockCollideAction DefaultSolidCollideAction() {
                return (ne,nasBlock,headSurrounded,x,y,z) => {
                    if (headSurrounded) {
                        //if (ne.GetType() == typeof(NasPlayer)) {
                        //    NasPlayer np = (NasPlayer)ne;
                        //    np.p.Message("head surrounded @ {0} {1} {2}", x, y, z);
                        //}
                        ne.TakeDamage(2f, NasEntity.DamageSource.Suffocating);
                        
                    }
                    
                };
            }
            
            public static NasBlockCollideAction LavaCollideAction() {
                return (ne,nasBlock,headSurrounded,x,y,z) => {
                    if (headSurrounded) {
                        ne.holdingBreath = true;
                    }
                    ne.TakeDamage(2.5f, NasEntity.DamageSource.Suffocating, "@p %cmelted in lava.");
                };
            }
            
            
            public static NasBlockCollideAction LiquidCollideAction() {
                return (ne,nasBlock,headSurrounded,x,y,z) => {
                    if (headSurrounded) {
                        ne.holdingBreath = true;
                    }
                };
            }
            public static NasBlockCollideAction AirCollideAction() {
                return (ne,nasBlock,headSurrounded,x,y,z) => {
                    if (headSurrounded) {
                        ne.holdingBreath = false;
                    }
                };
            }
        
    }

}
