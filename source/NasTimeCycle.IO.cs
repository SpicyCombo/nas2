/* using MCGalaxy;
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

        public static string sday;
        public static string smin;
        public static DayCycles scycle;

        public static void StoreTimeData(int day, float min, DayCycles cycle)
        {
            if (File.Exists(Nas2.Path))
            {
                cyc.sday = day;
                using (StreamWriter sw = new StreamWriter(TimeFilePath)) // To help you better understand, this is the stream writer
                using (JsonWriter writer = new JsonTextWriter(sw)) // this is the json writer that will help me to serialize and deserialize items in the file
                {
                    serializer.Serialize(writer, "a");
                }

            }
            else
            {
                File.Create(TimeFilePath);
                Logger.Log(LogType.Debug, "Created new json time file " + TimeFilePath + " !");
            }
        }
    }
}*/