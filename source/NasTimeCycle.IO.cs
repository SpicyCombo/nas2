using MCGalaxy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle {
        static NasTimeCycle cyc = new NasTimeCycle();

        public int day = 0;
        public int minutes = 7*hourMinutes;
        public DayCycles cycle = DayCycles.Sunrise;

        public static void StoreTimeData(int day, int minutes, DayCycles cycle)
        {
            cyc.day = day;
            cyc.minutes = minutes;
            cyc.cycle = cycle;

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
            else
            {
                //string jsonString = File.ReadAllText(TimeFilePath);
                using (StreamWriter sw = new StreamWriter(TimeFilePath)) // To help you better understand, this is the stream writer
                using (JsonWriter writer = new JsonTextWriter(sw)) // this is the json writer that will help me to serialize and deserialize items in the file
                {
                    serializer.Serialize(writer, cyc);
                }
            }
        }
    }
}