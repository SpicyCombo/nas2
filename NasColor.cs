using System;
using System.IO;
using System.Drawing;
using System.Text;
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public class DynamicColor {
        static SchedulerTask task;
        public static ColorDesc[] defaultColors;
        public static ColorDesc[] fullHealthColors;
        public static ColorDesc[] mediumHealthColors;
        public static ColorDesc[] lowHealthColors;
        public static ColorDesc[] direHealthColors;
        const string selectorImageName = "selectorColors.png";
        public static bool Setup() {
            if (File.Exists("plugins/" + selectorImageName)) {
                File.Move("plugins/" + selectorImageName, Nas.Path + selectorImageName);
            }
            if (!File.Exists(Nas.Path + selectorImageName)) {
                Player.Console.Message("Could not locate {0} (needed for tool health/selection colors)", selectorImageName);
                return false;
            }
            
            Bitmap colorImage;
            colorImage = new Bitmap(Nas.Path + "selectorColors.png");
            
            defaultColors = new ColorDesc[colorImage.Width];
            fullHealthColors = new ColorDesc[colorImage.Width];
            mediumHealthColors = new ColorDesc[colorImage.Width];
            lowHealthColors = new ColorDesc[colorImage.Width];
            direHealthColors = new ColorDesc[colorImage.Width];

            int index = 0;
            SetupDescs(index++, colorImage, ref defaultColors);
            SetupDescs(index++, colorImage, ref fullHealthColors);
            SetupDescs(index++, colorImage, ref mediumHealthColors);
            SetupDescs(index++, colorImage, ref lowHealthColors);
            SetupDescs(index++, colorImage, ref direHealthColors);
            colorImage.Dispose();

            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(100));
            return true;
        }
        static void SetupDescs(int yOffset, Bitmap colorImage, ref ColorDesc[] colorDescs) {
            for (int i = 0; i < colorImage.Width; i++) {
                Color color = colorImage.GetPixel(i, yOffset);
                colorDescs[i].R = color.R;
                colorDescs[i].G = color.G;
                colorDescs[i].B = color.B;
                colorDescs[i].A = 255;
                colorDescs[i].Code = 'h';
                colorDescs[i].Fallback = 'f';
            }
        }
        public static void TakeDown() {
            if (task == null) return;
            Server.MainScheduler.Cancel(task);
        }

        static int index;
        static void Update(SchedulerTask task) {
            index = (index + 1) % defaultColors.Length;
            Player[] players = PlayerInfo.Online.Items;

            foreach (Player p in players) {
                if (!p.Supports(CpeExt.TextColors)) continue;
                NasPlayer np = NasPlayer.GetNasPlayer(p);
                if (np == null) {
                    //p.Message("your NP is null");
                    continue;
                }
                ColorDesc desc = np.inventory.selectorColors[index];
                //p.Message("Sending the color desc {0} {1} {2} {3}", desc.R, desc.G, desc.B, desc.Code);
                p.Send(Packet.SetTextColor(desc));
            }
        }
    }

}
