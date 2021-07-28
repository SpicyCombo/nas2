using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;

namespace NotAwesomeSurvival {

    public partial class NasLevel {
        public const string Path = Nas.Path + "leveldata/";
        const string Extension = ".json";

        public static void Setup() {
            OnLevelLoadedEvent.Register(OnLevelLoaded, Priority.High);
            OnLevelUnloadEvent.Register(OnLevelUnload, Priority.Low);
            OnLevelDeletedEvent.Register(OnLevelDeleted, Priority.Low);
            OnLevelRenamedEvent.Register(OnLevelRenamed, Priority.Low);
        }
        public static void TakeDown() {
            OnLevelLoadedEvent.Unregister(OnLevelLoaded);
            OnLevelUnloadEvent.Unregister(OnLevelUnload);
            OnLevelDeletedEvent.Unregister(OnLevelDeleted);
            OnLevelRenamedEvent.Unregister(OnLevelRenamed);
            
            Level[] loadedLevels = LevelInfo.Loaded.Items;
            foreach (Level lvl in loadedLevels) {
                if (!all.ContainsKey(lvl.name)) { return; }
                Unload(lvl.name, all[lvl.name]);
            }
        }
        public static string GetFileName(string name) {
            return Path + name + Extension;
        }
        public static NasLevel Get(string name) {
            if (all.ContainsKey(name)) {
                return all[name];
            }
            return null;
        }
        public static void Unload(string name, NasLevel nl) {
            nl.EndTickTask();
            string jsonString;
            jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented);
            string fileName = GetFileName(name);
            File.WriteAllText(fileName, jsonString);
            Logger.Log(LogType.Debug, "Unloaded(saved) NasLevel " + fileName + "!");
            all.Remove(name);
        }
        public static void OnLevelLoaded(Level lvl) {
            if (NasBlock.blocksIndexedByServerBlockID == null) {
                //Player.Console.Message("This has to be filled in before NasLevels can work");
                return;
            }
            //Player.Console.Message("CALLING OnLevelLoaded for {0}", lvl.name);
            NasLevel nl = new NasLevel();
            string fileName = GetFileName(lvl.name);
            if (File.Exists(fileName)) {
                string jsonString = File.ReadAllText(fileName);
                nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                nl.lvl = lvl;
                all.Add(lvl.name, nl);
                nl.BeginTickTask();
                Logger.Log(LogType.Debug, "Loaded NasLevel " + fileName + "!");
            }
        }
        static void OnLevelUnload(Level lvl, ref bool cancel) {
            if (!all.ContainsKey(lvl.name)) { return; }
            Unload(lvl.name, all[lvl.name]);
        }
        static void OnLevelDeleted(string name) {
            string fileName = Path + name + Extension;
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log(LogType.Debug, "Deleted NasLevel " + fileName + "!");
            }
        }
        static void OnLevelRenamed(string srcMap, string dstMap) {
            string fileName = Path + srcMap + Extension;
            if (File.Exists(fileName)) {
                string newFileName = Path + dstMap + Extension;
                File.Move(fileName, newFileName);
                Logger.Log(LogType.Debug, "Renamed NasLevel " + fileName + " to " + newFileName + "!");
                //Unload(srcMap, all[srcMap]);
            }
        }
    }

}
