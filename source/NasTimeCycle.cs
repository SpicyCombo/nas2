using MCGalaxy;
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
using MCGalaxy.Events;
using MCGalaxy.Events.ServerEvents;

namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle {

        // Vars
        public static float globalCurrentTime;
        public static DayCycles globalCurrentDayCycle;
        public static float staticMaxTime;
        static SchedulerTask task;
        public static int gameday = 0; // just defining it here, starting at 0. then adding on to it :)
        public static string TimeFilePath = Nas2.CoreSavePath + "time.json";
        static JsonSerializer serializer = new JsonSerializer();

        public static string globalSkyColor; // self explanatory
        public static string globalCloudColor;
        public static string globalSunColor;

        // Cycle Settings
        public static DayCycles dayCycle = DayCycles.Sunrise; // default cycle
        public static int cycleCurrentTime = 0; // current cycle time (must be zero to start)
        public static int cycleMaxTime = 14400; // duration a whole day
        public static int hourMinutes = 600; //seconds in an hour

        public enum DayCycles // Enum with day and night cycles
        {
            Sunrise, Day, Sunset, Night, Midnight
        }



        public static void Setup()
        {
            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromSeconds(1));
            dayCycle = DayCycles.Sunrise; // start with sunrise state
            // Static variables to keep time after switching scenes

            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Logger.Log(LogType.Debug, "Created new json time file " + TimeFilePath + " !");
                using (StreamWriter sw = new StreamWriter(TimeFilePath)) // To help you better understand, this is the stream writer
                using (JsonWriter writer = new JsonTextWriter(sw)) // this is the json writer that will help me to serialize and deserialize items in the file
                {
                    serializer.Serialize(writer, cyc);
                }
            }

            // cycleCurrentTime = serializer.Deserialize(reader)
            string jsonString = File.ReadAllText(TimeFilePath);
            NasTimeCycle ntc = JsonConvert.DeserializeObject<NasTimeCycle>(jsonString);
            dayCycle = ntc.cycle;
            gameday = ntc.day;
            cycleCurrentTime = ntc.minutes;

            gameday = cyc.day;
            cycleCurrentTime = cyc.minutes;
            dayCycle = cyc.cycle;
            
            staticMaxTime = cycleMaxTime;
        }

        public static void TakeDown()
        {
            Server.MainScheduler.Cancel(task);
        }

        static void Update(SchedulerTask task) // this gets executed each time a second has passed.
        {
            // Update cycle time
            cycleCurrentTime += 12;

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

            //when to change cycles
            if (cycleCurrentTime == 7 * hourMinutes & cycleCurrentTime < 8 *hourMinutes) dayCycle = DayCycles.Sunrise; // 7am
            if (cycleCurrentTime >= 8 * hourMinutes & cycleCurrentTime < 18*hourMinutes) dayCycle = DayCycles.Day; // 8am
            if (cycleCurrentTime >= 18 * hourMinutes & cycleCurrentTime < 20*hourMinutes) dayCycle = DayCycles.Sunset; // 6pm
            if (cycleCurrentTime >= 20 * hourMinutes & cycleCurrentTime < 24*hourMinutes) dayCycle = DayCycles.Night; // 8pm
            if (cycleCurrentTime == 24 * hourMinutes | cycleCurrentTime == 0 | cycleCurrentTime < 7*hourMinutes) dayCycle = DayCycles.Sunrise; // 0 am

            // Sunrise state (you can do a lot of stuff based on every cycle state, like enable monster spawning only when dark)
            if (dayCycle == DayCycles.Sunrise)
            {
                globalCloudColor = "#ff8c00"; // Dark Orange
                globalSkyColor = "#FFA500"; // Orange
                globalSunColor = "#a9a9a9"; // Dark Gray
            }

            // Mid Day state
            if (dayCycle == DayCycles.Day)
            {
                globalCloudColor = "#ffffff"; // white
                globalSkyColor = "#ADD8E6"; // light blue
                globalSunColor = "#ffffff"; // white
            }

            // Sunset state
            if (dayCycle == DayCycles.Sunset)
            {
                globalCloudColor = "#ff8c00"; // Dark Orange
                globalSkyColor = "#FFA500"; // Orange
                globalSunColor = "#a9a9a9"; // Dark Gray
            }

            // Night state
            if (dayCycle == DayCycles.Night)
            {
                globalCloudColor = "#808080"; // grey
                globalSkyColor = "#404040"; // darko grey
                globalSunColor = "#808080"; // grey
            }

            // Midnight state
            if (dayCycle == DayCycles.Midnight)
            {
                globalCloudColor = "#404040"; // darko grey
                globalSkyColor = "#000000"; // black
                globalSunColor = "#404040"; // darko grey
            }

            UpdateEnvSettings(globalCloudColor, globalSkyColor, globalSunColor);
            StoreTimeData(gameday, cycleCurrentTime, dayCycle);
        }

        static void UpdateEnvSettings(string cloud, string sky, string sun)
        {
            // THANKS UNK
            Level[] loaded = LevelInfo.Loaded.Items; // We are creating a list (levels)

            foreach (Level lvl in loaded) // For each, think of this as for... in... in python
            {
                lvl.Config.LightColor = sun; // Sun Colour
                lvl.Config.CloudColor = cloud; // Cloud Colour
                lvl.Config.SkyColor = sky; // Sky
                lvl.SaveSettings(); // We save these settings after
            }
        }

    }
}