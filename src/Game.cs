using Foster.Framework;
using FosterPlatformer.Components;
using FosterPlatformer.Extensions;
using System;
using System.Diagnostics;
using System.Numerics;

namespace FosterPlatformer
{
    public class Game : Module
    {
        public static readonly int Width = 240;
        public static readonly int Height = 136;
        public static readonly int TileWidth = 8;
        public static readonly int TileHeight = 8;
        public static readonly int Columns = Width / TileWidth;
        public static readonly int Rows = Height / TileHeight;

        public static readonly string Title = "SWORD II: ADVENTURE OF FROG";
        public static readonly string Controls = "arrow keys + X / C\nstick + A / X";
        public static readonly string Ending = "YOU SAVED POND\nAND YOU ARE\nA REAL HERO";

        public World world;
        public FrameBuffer Buffer;
        public Batch2D Batch = new Batch2D();
        public Point2 Room;
        public Vector2 Camera;
        public bool Fullscreen = false;

        private bool drawColliders;
        private bool transition = false;
        private const float transitionDuration = 0.4f;
        private float nextEase;
        private Point2 nextRoom = new Point2(0, 0);
        private Point2 lastRoom = new Point2(0, 0);
        // Vector<Entity> lastEntities;
        private Point2 shake;
        private float shakeTimer = 0;

        // This is called when the Application has Started.
        protected override void Startup()
        {
            // Add a Callback to the Primary Window's Render loop.
            // By Default a single Window is created at startup
            // Alternatively App.System.Windows has a list of all open windows.
            App.Window.OnRender += Render;

            world = new World();
            world.game = this;

            // Load content.
            Content.Load();

            // Set batcher to use Nearest Filter.
            Texture.DefaultTextureFilter = TextureFilter.Nearest;

            // Framebuffer for the game.
            Buffer = new FrameBuffer(Width, Height);

            drawColliders = false;

            // Load first room.
            LoadRoom(new Point2(0, 0));
            Camera = new Vector2(Room.X * Width, Room.Y * Height);
            Fullscreen = false;
        }

        //
        public void LoadRoom(Point2 cell, bool isReload = false)
        {
            Bitmap grid = Content.FindRoom(cell);
            Debug.Assert(grid != null, "Room doesn't exist!");
            Room = cell;

            // Get room offset.
            Point2 offset = new Point2(cell.X * Width, cell.Y * Height);

            // Get the castle tileset for now.
            var castle = Content.FindTileset("castle");
            var grass = Content.FindTileset("grass");
            var plants = Content.FindTileset("plants");
            var backs = Content.FindTileset("back");
            var jumpthrus = Content.FindTileset("jumpthru");

            // Make the floor.
            var floor = world.AddEntity(offset);
            var tilemap = floor.Add<Tilemap>(new Tilemap(8, 8, Columns, Rows));
            var solids = floor.Add<Collider>(Collider.MakeGrid(8, 40, 23));
            solids.Mask = Mask.Solid;

            // Loop over the room grid.
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                {
                    Point2 worldPosition = offset + new Point2(x * TileWidth, y * TileHeight) + new Point2(TileWidth / 2, TileHeight);
                    Color col = grid.Pixels[x + y * Columns];
                    UInt32 rgb =
                        ((UInt32)col.R << 16) |
                        ((UInt32)col.G << 8) |
                        ((UInt32)col.B);

                    switch (rgb) {
                        // Black does nothing.
                        case 0x000000:
                        break;

                        // Castle tiles.
                        case 0xffffff:
                            tilemap.SetCell(x, y, castle.RandomTile());
                            solids.SetCell(x, y, true);
                        break;

                        // Grass tiles.
                        case 0x8f974a:
                            tilemap.SetCell(x, y, grass.RandomTile());
                            solids.SetCell(x, y, true);
                        break;

                        // Plants tiles.
                        case 0x4b692f:
                            tilemap.SetCell(x, y, plants.RandomTile());
                        break;

                        // Back tiles.
                        case 0x45283c:
                            tilemap.SetCell(x, y, backs.RandomTile());
                        break;

                        // Jumpthru tiles.
                        case 0xdf7126:
                            tilemap.SetCell(x, y, jumpthrus.RandomTile());
                            var jumpthruEn = world.AddEntity(offset + new Point2(x * TileWidth, y * TileHeight));
                            var jumpthruCol = jumpthruEn.Add<Collider>(Collider.MakeRect(new RectInt(0, 0, 8, 4)));
                            jumpthruCol.Mask = Mask.Jumpthru;
                        break;

                        // Player (only if it doesn't already exist)
                        case 0x6abe30:
                            // @TODO
                        break;

                        // @TODO: Remaining entities.
                    }
                }
        }

        // This is called when the Application is shutting down
        // (or when the Module is removed).
        protected override void Shutdown()
        {
            // Remove our Callback
            App.Window.OnRender -= Render;
        }

        // This is called every frame of the Application.
        protected override void Update()
        {
            // Toggle collider render.
            if (App.Input.Keyboard.Pressed(Keys.F1)) {
                drawColliders = !drawColliders;
            }

            // Reload current room.
            if (App.Input.Keyboard.Pressed(Keys.F2)) {
                transition = false;
                // world.clear();
                LoadRoom(Room);
            }

            // Reload first room.
            if (App.Input.Keyboard.Pressed(Keys.F9)) {
                transition = false;
                // world.clear();
                LoadRoom(new Point2(0, 0));
            }

            // Toggle Fullscreen.
            if (App.Input.Keyboard.Pressed(Keys.F4)) {
                App.Window.Fullscreen = Fullscreen = !Fullscreen;
            }

            // Exit the game.
            if (App.Input.Keyboard.Pressed(Keys.Escape)) {
                App.Exit();
            }

            // Normal Update
            if (!transition) {
                // Screen shake.
                shakeTimer -= Time.Delta;

                if (shakeTimer > 0) {
                    if (Time.OnInterval(0.05f)) {
                        // @TODO
                    }
                }
                else
                    shake = Point2.Zero;

                // Update objects.
                world.Update();
            }
            // Room Transition routine
            else {
                // Increment ease.
                nextEase = Calc.Approach(nextEase, 1.0f, Time.Delta / transitionDuration);

                // Get last and next camera position.
                var lastCam = new Vector2(lastRoom.X * Width, lastRoom.Y * Height);
                var nextCam = new Vector2(nextRoom.X * Width, nextRoom.Y * Height);

                // LERP camera position.
                Camera = lastCam + (nextCam - lastCam) * Ease.CubeInOut(nextEase);

                // Finish transition.
                if (nextEase >= 1.0f) {
                    // Boost player on vertical up rooms.
                    if (nextRoom.Y < lastRoom.Y) {
                        // @TODO
                    }

                    // Delete old objects (except player!)
                    // @TODO

                    // Time.PauseFor(0.1f);
                    transition = false;
                }
            }
        }

        private void Render(Window window)
        {
            #region Draw gameplay stuff.

            App.Graphics.Clear(Buffer, Color.FromHexStringRGB("#150e22"));

            // Push camera offset.
            Batch.PushMatrix(Matrix3x2.CreateTranslation(-Camera + shake));

            // Draw gameplay objects.
            world.Render(Batch);

            // Draw debug colliders.
            if (drawColliders) {
                var collider = world.First<Collider>();

                while (collider != null) {
                    collider.Render(Batch);
                    collider = (Collider) collider.Next;
                }
            }

            // End camera offset.
            Batch.PopMatrix();

            // Draw the health.
            // @TODO

            // Draw the gameplay buffer.
            Batch.Render(Buffer);
            Batch.Clear();

            #endregion

            #region Draw buffer to the screen.

            float scale = Math.Min(
                App.Window.RenderWidth / (float) Buffer.RenderWidth,
                App.Window.RenderHeight / (float) Buffer.RenderHeight
            );

            Vector2 screenCenter = new Vector2(App.Window.RenderWidth, App.Window.RenderHeight) / 2;
            Vector2 bufferCenter = new Vector2(Buffer.RenderWidth, Buffer.RenderHeight) / 2;

            App.Graphics.Clear(window, Color.Black);
            Batch.PushMatrix(Mat3x2Ext.CreateTransform(screenCenter, bufferCenter, Vector2.One * scale, 0));
            Batch.Image(Buffer.Attachments[0], Vector2.Zero, Color.White);
            Batch.PopMatrix();
            Batch.Render(window);
            Batch.Clear();

            #endregion
        }

        public void Shake(float time)
        {
            shakeTimer = time;
        }
    }
}
