using Foster.Framework;
using System;
using System.Diagnostics;
using System.Numerics;

namespace FosterPlatformer
{
    public class Collider : Component
    {
        public enum ShapeType
        {
            None,
            Rect,
            Grid
        };

        public int Mask = 0;

        private struct Grid
        {
            public int Columns;
            public int Rows;
            public int TileSize;
            public bool[] cells;
        }

        private ShapeType shape = ShapeType.None;
        private RectInt rect;
        private Grid grid;

        public Collider()
        {
            Visible = false;
            Active = false;
        }

        public static Collider MakeRect(RectInt rect)
        {
            Collider collider = new Collider();
            collider.shape = ShapeType.Rect;
            collider.rect = rect;
            return collider;
        }

        public static Collider MakeGrid(int tileSize, int columns, int rows)
        {
            Collider collider = new Collider();
            collider.shape = ShapeType.Grid;
            collider.grid.TileSize = tileSize;
            collider.grid.Columns = columns;
            collider.grid.Rows = rows;
            collider.grid.cells = new bool[columns * rows];
            return collider;
        }

        public ShapeType Shape()
        {
            return shape;
        }

        public RectInt GetRect()
        {
            Debug.Assert(shape == ShapeType.Rect, "Collider is not a Rectangle.");
            return rect;
        }

        public void SetRect(RectInt value)
        {
            Debug.Assert(shape == ShapeType.Rect, "Collider is not a Rectangle.");
            rect = value;
        }

        public bool GetCell(int x, int y)
        {
            Debug.Assert(shape == ShapeType.Grid, "Collider is not a Grid.");
            Debug.Assert(x >= 0 && y >= 0 && x < grid.Columns && y < grid.Rows, "Cell is out of bounds.");

            return grid.cells[x + y * grid.Columns];
        }

        public void SetCell(int x, int y, bool value)
        {
            Debug.Assert(shape == ShapeType.Grid, "Collider is not a Grid.");
            Debug.Assert(x >= 0 && y >= 0 && x < grid.Columns && y < grid.Rows, "Cell is out of bounds.");

            grid.cells[x + y * grid.Columns] = value;
        }

        public void SetCells(int x, int y, int w, int h, bool value)
        {
            for (int tx = x; tx < x + w; tx++)
                for (int ty = y; ty < y + h; ty++)
                    grid.cells[tx + ty * grid.Columns] = value;
        }

        public bool Check(int mask, Point2 offset)
        {
            return First(mask, offset) != null;
        }

        public Collider First(int mask, Point2 offset)
        {
            if (World() != null) {
                var other = World().First<Collider>();

                while (other != null) {
                    if (other != this &&
                        (other.Mask & mask) == mask &&
                        Overlaps(other, offset))
                        return other;

                    other = (Collider) other.Next;
                }
            }

            return null;
        }

        public bool Overlaps(Collider other, Point2 offset)
        {
            if (shape == ShapeType.Rect) {
                if (other.shape == ShapeType.Rect) {
                    return RectToRect(this, other, offset);
                }
                else if (other.shape == ShapeType.Grid) {
                    return RectToGrid(this, other, offset);
                }
            }
            else if (shape == ShapeType.Grid) {
                if (other.shape == ShapeType.Rect) {
                    return RectToRect(other, this, -offset);
                }
                else if (other.shape == ShapeType.Grid) {
                    Debug.Assert(false, "Grid->Grid Overlap checks not supported!");
                }
            }

            return false;
        }

        public override void Render(Batch2D batch)
        {
            Color color = Color.Red;

            batch.PushMatrix(Matrix3x2.CreateTranslation(Entity.Position));

            if (shape == ShapeType.Rect) {
                batch.HollowRect(rect, 1, color);
            }
            else if (shape == ShapeType.Grid) {
                for (int x = 0; x < grid.Columns; x++) {
                    for (int y = 0; y < grid.Rows; y++) {
                        if (!grid.cells[x + y * grid.Columns])
                            continue;

                        batch.HollowRect(
                            new Rect(x * grid.TileSize, y * grid.TileSize, grid.TileSize, grid.TileSize),
                            1, color
                        );
                    }
                }
            }

            batch.PopMatrix();
        }

        private static bool RectToRect(Collider a, Collider b, Point2 offset)
        {
            RectInt ar = a.rect + a.Entity.Position + offset;
            RectInt br = b.rect + b.Entity.Position;

            return ar.Overlaps(br);
        }

        private static bool RectToGrid(Collider a, Collider b, Point2 offset)
        {
            // Get a relative rectangle to the grid.
            RectInt rect = a.rect + a.Entity.Position + offset - b.Entity.Position;

            // Get the cells the rectangle overlaps.
            int left = Calc.Clamp((int) Math.Floor(rect.X / (float) b.grid.TileSize), 0, b.grid.Columns);
            int right = Calc.Clamp((int) Math.Ceiling(rect.Right / (float) b.grid.TileSize), 0, b.grid.Columns);
            int top = Calc.Clamp((int) Math.Floor(rect.Y / (float) b.grid.TileSize), 0, b.grid.Rows);
            int bottom = Calc.Clamp((int) Math.Ceiling(rect.Bottom / (float) b.grid.TileSize), 0, b.grid.Rows);

            // Check each cell.
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                    if (b.grid.cells[x + y * b.grid.Columns])
                        return true;

            // All cells were empty.
            return false;
        }
    }
}
