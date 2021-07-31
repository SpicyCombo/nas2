using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;
using Priority_Queue;

namespace NotAwesomeSurvival {

    public partial class NasLevel {
        static Scheduler TickScheduler;
        static TimeSpan tickDelay = TimeSpan.FromMilliseconds(100);
        static Random r = new Random();
        public class BlockLocation {
            public int X, Y, Z;
            public BlockLocation() { }
            public BlockLocation(QueuedBlockUpdate qb) {
                X = qb.x; Y = qb.y; Z = qb.z;
            }
            public BlockLocation(int x, int y, int z) {
                X = x; Y = y; Z = z;
            }
        }
        public struct QueuedBlockUpdate {
            public int x, y, z;
            public NasBlock nb;
            public DateTime date;
        }
        
        [JsonIgnore] public static Dictionary<string, NasLevel> all = new Dictionary<string, NasLevel>();
        [JsonIgnore] public Level lvl;
        public ushort[,] heightmap;
        public List<BlockLocation> blocksThatMustBeDisturbed = new List<BlockLocation>();
        public Dictionary<string, NasBlock.Entity> blockEntities = new Dictionary<string, NasBlock.Entity>();
        [JsonIgnore] public SimplePriorityQueue<QueuedBlockUpdate, DateTime> tickQueue = new SimplePriorityQueue<QueuedBlockUpdate, DateTime>();
        [JsonIgnore] public SchedulerTask schedulerTask;
        
        public void BeginTickTask() {
            if (TickScheduler == null) TickScheduler = new Scheduler("NasLevelTickScheduler");
            
            Player.Console.Message("Re-disturbing {0} blocks.", blocksThatMustBeDisturbed.Count);
            foreach (BlockLocation blockLoc in blocksThatMustBeDisturbed) {
                DisturbBlock(blockLoc.X, blockLoc.Y, blockLoc.Z);
            }
            blocksThatMustBeDisturbed.Clear();
            schedulerTask = TickScheduler.QueueRepeat(TickLevelCallback, this, tickDelay);
        }
        // /newlvl eb_0,0 384 256 384 nasgen ee
        public void EndTickTask() {
            if (TickScheduler == null) TickScheduler = new Scheduler("NasLevelTickScheduler");
            TickScheduler.Cancel(schedulerTask);
            
            Player.Console.Message("Saving {0} blocks to re-disturb later.", tickQueue.Count);
            if (tickQueue.Count == 0) { return; }
            
            blocksThatMustBeDisturbed = new List<BlockLocation>();
            foreach (QueuedBlockUpdate qb in tickQueue) {
                BlockLocation blockLoc = new BlockLocation(qb);
                if (blocksThatMustBeDisturbed.Contains(blockLoc)) { continue; }
                blocksThatMustBeDisturbed.Add(blockLoc);
            }
            tickQueue.Clear();
        }
        static void TickLevelCallback(SchedulerTask task) {
            NasLevel nl = (NasLevel)task.State;
            nl.Tick();
        }
        public void Tick() {
            if (tickQueue.Count < 1) { return; }
            int actions = 0;
            while (tickQueue.First.date < DateTime.UtcNow) {
                if (actions > 64) {
                    //Player.Console.Message("falling behind on ticks");
                    break;
                }
                QueuedBlockUpdate qb = tickQueue.First;
                if (NasBlock.blocksIndexedByServerBlockID[lvl.GetBlock((ushort)qb.x, (ushort)qb.y, (ushort)qb.z)].selfID == qb.nb.selfID) {
                    qb.nb.disturbedAction(this, qb.x, qb.y, qb.z);
                }
                tickQueue.Dequeue();
                actions++;
                if (tickQueue.Count < 1) { break; }
            }
        }
        
        public void SetBlock(int x, int y, int z, BlockID serverBlockID, bool disturbDiagonals = false) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            lvl.Blockchange((ushort)x, (ushort)y, (ushort)z, serverBlockID);
            DisturbBlocks(x, y, z, disturbDiagonals);
        }
        
        public void SimulateSetBlock(int x, int y, int z, bool disturbDiagonals = false) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            DisturbBlocks(x, y, z, disturbDiagonals);
        }
        public void DisturbBlocks(int x, int y, int z, bool diagonals = false) {
            if (diagonals) {
                for (int xOff = -1; xOff <= 1; xOff++)
                    for (int yOff = -1; yOff <= 1; yOff++)
                        for (int zOff = -1; zOff <= 1; zOff++)
                {
                    DisturbBlock(x+xOff, y+yOff, z+zOff);
                }
                return;
            }
            DisturbBlock(x, y, z);
            
            DisturbBlock(x+1, y, z);
            DisturbBlock(x-1, y, z);
            
            DisturbBlock(x, y+1, z);
            DisturbBlock(x, y-1, z);
            
            DisturbBlock(x, y, z+1);
            DisturbBlock(x, y, z-1);
        }
        /// <summary>
        /// Call to make the nasBlock at this location queue its "whatHappensWhenDisturbed" function.
        /// </summary>
        private void DisturbBlock(int x, int y, int z) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            NasBlock nb = NasBlock.blocksIndexedByServerBlockID[lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z)];
            if (nb.disturbedAction == null) { return; }
            QueuedBlockUpdate qb = new QueuedBlockUpdate();
            qb.x = x;
            qb.y = y;
            qb.z = z;
            float seconds = (float)(r.NextDouble() * (nb.disturbDelayMax - nb.disturbDelayMin) + nb.disturbDelayMin);
            qb.date = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
            qb.date = qb.date.Floor(tickDelay);
            qb.nb = nb;
            //lvl.Message("queueing thing "+qb.date.ToString("hh:mm:ss.fff tt"));
            tickQueue.Enqueue(qb, qb.date);
        }
        public BlockID GetBlock(int x, int y, int z) {
            return lvl.GetBlock((ushort)x, (ushort)y, (ushort)z);
        }

    }
    
    public static class DateExtensions {
        public static DateTime Round(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks + (span.Ticks / 2) + 1)/ span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Floor(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Ceil(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }

}
