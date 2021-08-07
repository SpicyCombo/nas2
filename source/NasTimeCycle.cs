/* using MCGalaxy;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle {

        // Vars
        public static float globalCurrentTime;
        public static DayCycles globalCurrentDayCycle;
        public static float staticMaxTime;
        static SchedulerTask task;
        public static int gameday; // just defining it here, starting at 0. then adding on to it :)
        public static string TimeFilePath = Nas2.CoreSavePath + "time.json";
        static JsonSerializer serializer = new JsonSerializer();

        public static string globalSkyColor;
        public static string globalCloudColor;
        public static string globalSunColor;

        // Cycle Settings
        public static DayCycles dayCycle = DayCycles.Sunrise; // default cycle
        public static float cycleCurrentTime = 0; // current cycle time (must be zero to start)
        public static float cycleMaxTime = 1440; // duration of cycle (in-game minutes)

        public enum DayCycles // Enum with day and night cycles
        {
            Sunrise, Day, Sunset, Night, Midnight
        }



        public static void Setup()
        {
            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromSeconds(1));
            dayCycle = DayCycles.Sunrise; // start with sunrise state
            // Static variables to keep time after switching scenes
            cycleCurrentTime = 0;
            dayCycle = globalCurrentDayCycle;

            staticMaxTime = cycleMaxTime;
        }

        public static void TakeDown()
        {
            Server.MainScheduler.Cancel(task);
        }

        static void Update(SchedulerTask task) // this gets executed each time a second has passed.
        {
            // Update cycle time
            cycleCurrentTime += (float)1.2;

            // Static variables to keep time after switching scenes
            globalCurrentTime = cycleCurrentTime;
            globalCurrentDayCycle = dayCycle;

            // Check if cycle time reach cycle duration time
            if (cycleCurrentTime >= cycleMaxTime)
            {
                cycleCurrentTime = 0; // back to 0 (restarting cycle time)
                gameday += 1; // one more in-game day just passed :p
                dayCycle++; // change cycle state
            }

            // If reach final state we back to sunrise (Enum id 0)
            if (dayCycle > DayCycles.Midnight) dayCycle = DayCycles.Sunrise;

            // Sunrise state (you can do a lot of stuff based on every cycle state, like enable monster spawning only when dark)
            if (dayCycle == DayCycles.Sunrise)
            {

            }

            // Mid Day state
            if (dayCycle == DayCycles.Day)
            {

            }

            // Sunset state
            if (dayCycle == DayCycles.Sunset)
            {

            }

            // Night state
            if (dayCycle == DayCycles.Night)
            {

            }

            // Midnight state
            if (dayCycle == DayCycles.Midnight)
            {

            }

            StoreTimeData(gameday, cycleCurrentTime, dayCycle);
        }
    }
}
 */