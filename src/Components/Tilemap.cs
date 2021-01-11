using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class Tilemap : Component
    {
        private Subtexture[] grid;
        private int tileWidth = 0;
        private int tileHeight = 0;
        private int columns = 0;
        private int rows = 0;

        public Tilemap(int tileWidth, int tileHeight, int columns, int rows)
        {
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.columns = columns;
            this.rows = rows;
            this.grid = new Subtexture[columns * rows];
        }

        public int TileWidth()
        {
            return tileWidth;
        }

        public int TileHeight()
        {
            return tileHeight;
        }

        public int Columns()
        {
            return columns;
        }

        public int Rows()
        {
            return rows;
        }

        public void SetCell(int x, int y, Subtexture tex)
        {
            grid[x + y * columns] = tex;
        }

        public void SetCells(int x, int y, int w, int h, Subtexture tex)
        {
            for (int tx = x; tx < x + w; tx++)
                for (int ty = y; ty < y + h; ty++)
                    SetCell(tx, ty, tex);
        }

        public override void Render(Batch2D batch)
        {
            batch.PushMatrix(Matrix3x2.CreateTranslation(Entity.Position));

            for (int x = 0; x < columns; x++)
                for (int y = 0; y < rows; y++)
                    if (grid[x + y * columns] != null && grid[x + y * columns].Texture != null) {
                        batch.Image(grid[x + y * columns], new Vector2(x * tileWidth, y * tileHeight), Color.White);
                    }

            batch.PopMatrix();
        }
    }
}
