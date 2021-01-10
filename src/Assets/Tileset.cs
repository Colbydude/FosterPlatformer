using Foster.Framework;
using System;

namespace FosterPlatformer.Assets
{
    public class Tileset
    {
        public static readonly int MaxColumns = 16;
        public static readonly int MaxRows = 16;

        public string Name;
        public int Columns = 0;
        public int Rows = 0;
        public Subtexture[] Tiles = new Subtexture[MaxColumns * MaxRows];

        /// <summary>
        ///
        /// </summary>
        public Subtexture RandomTile()
        {
            return Rand.Instance.Choose<Subtexture>(Tiles);
        }
    }
}
