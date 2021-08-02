using System;
using MCGalaxy;
using BlockID = System.UInt16;
using MCGalaxy.Blocks;
using MCGalaxy.Generator.Foliage;

namespace NotAwesomeSurvival
{
    public sealed class SpruceTree : Tree
    {

        public override long EstimateBlocksAffected() { return height + size * size * size; }

        public override int DefaultSize(Random rnd) { return rnd.Next(5, 8); }

        // const BlockID leavesID = 250 | Block.Extended;
        // const BlockID logID = Block.Extended|146;

        public override void SetData(Random rnd, int value)
        {
            height = value;
            size = height - rnd.Next(2, 4);
            this.rnd = rnd;
        }

        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            for (ushort dy = 0; dy < height + size - 1; dy++)
                output(x, (ushort)(y + dy), z, Block.FromRaw(250) /*leaves*/);

            for (int dy = -size; dy <= size; ++dy)
                for (int dz = -size; dz <= size; ++dz)
                    for (int dx = -size; dx <= size; ++dx)
                    {
                        int dist = (int)(Math.Sqrt(dx * dx + dy * dy + dz * dz));
                        if ((dist < size + 1) && rnd.Next(dist) < 2)
                        {
                            ushort xx = (ushort)(x + dx), yy = (ushort)(y + dy + height), zz = (ushort)(z + dz);

                            if (xx != x || zz != z || dy >= size - 1)
                                output(xx, yy, zz, Block.FromRaw(146) /*log*/);
                        }
                    }
        }
    }

    public sealed class PeachTree : Tree
    {

        public override long EstimateBlocksAffected() { return height + size * size * size; }

        public override int DefaultSize(Random rnd) { return rnd.Next(5, 8); }

        // const BlockID leavesID = 250 | Block.Extended;
        // const BlockID logID = Block.Extended|146;

        public override void SetData(Random rnd, int value)
        {
            height = value;
            size = height - rnd.Next(2, 4);
            this.rnd = rnd;
        }

        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            for (ushort dy = 0; dy < height + size - 1; dy++)
                output(x, (ushort)(y + dy), z, Block.FromRaw(242) /*log*/);

            for (int dy = -size; dy <= size; ++dy)
                for (int dz = -size; dz <= size; ++dz)
                    for (int dx = -size; dx <= size; ++dx)
                    {
                        int dist = (int)(Math.Sqrt(dx * dx + dy * dy + dz * dz));
                        if ((dist < size + 1) && rnd.Next(dist) < 2)
                        {
                            ushort xx = (ushort)(x + dx), yy = (ushort)(y + dy + height), zz = (ushort)(z + dz);

                            if (xx != x || zz != z || dy >= size - 1)
                                output(xx, yy, zz, Block.FromRaw(103) /*leaves*/);
                        }
                    }
        }
    }
}