using Foster.Framework;
using FosterPlatformer.Components;
using FosterPlatformer.Extensions;
using System;
using System.Collections.Generic;
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
        public static readonly string Controls = "arrow keys + X / C\n\nstick + A / X";
        public static readonly string Ending = "YOU SAVED POND\n\nAND YOU ARE\n\nA REAL HERO";

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
        private List<Entity> lastEntities = new List<Entity>();
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
            world.Game = this;

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
            for (int x = 0; x < Columns; x++) {
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

                        // Player (only if it doesn't already exist).
                        case 0x6abe30:
                            if (world.First<Player>() == null)
                                Factory.Player(world, worldPosition + (isReload ? new Point2(0, -16) : Point2.Zero));
                        break;

                        // Brambles.
                        case 0xd77bba:
                            Factory.Bramble(world, worldPosition);
                        break;

                        // Spitter Plat.
                        case 0xac3232:
                            Factory.Spitter(world, worldPosition);
                        break;

                        // // Mosquito.
                        // case 0xfbf236:
                        //     Factory.Mosquito(world, worldPosition);
                        // break;

                        // // Door.
                        // case 0x9badb7:
                        //     Factory.Door(world, worldPosition);
                        // break;

                        // // Closing Door.
                        // case 0x847e87:
                        //     Factory.Door(world, worldPosition, !isReload);
                        // break;

                        // // Blob.
                        // case 0x3f3f74:
                        //     Factory.Blob(world, worldPosition);
                        // break;

                        // Ghost Frog.
                        case 0x76428a:
                            Factory.GhostFrog(world, worldPosition + new Point2(-4, 0));
                        break;
                    }
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
                world.Clear();
                LoadRoom(Room);
            }

            // Reload first room.
            if (App.Input.Keyboard.Pressed(Keys.F9)) {
                transition = false;
                world.Clear();
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
                        shake.X = Rand.Instance.Next(0, 2) == 0 ? -1 : 1;
                        shake.Y = Rand.Instance.Next(0, 2) == 0 ? -1 : 1;
                    }
                }
                else
                    shake = Point2.Zero;

                // Update objects.
                world.Update();

                // Check for transition / death.
                var player = world.First<Player>();
                if (player != null) {
                    var pos = player.Entity.Position;
                    var bounds = new RectInt(Room.X * Width, Room.Y * Height, Width, Height);

                    if (!bounds.Contains(pos)) {
                        // Target room.
                        Point2 nextRoom = new Point2(pos.X / Width, pos.Y / Height);
                        if (pos.X < 0) nextRoom.X--;
                        if (pos.Y < 0) nextRoom.Y--;

                        // See if room exists.
                        if (player.Health > 0 && Content.FindRoom(nextRoom) != null && nextRoom.X >= Room.X) {
                            Time.PauseFor(0.1f);

                            // Transition to it!
                            transition = true;
                            nextEase = 0;
                            this.nextRoom = nextRoom;
                            lastRoom = Room;

                            // Store entities from the previous room.
                            lastEntities.Clear();
                            Entity e = world.FirstEntity();

                            while (e != null) {
                                lastEntities.Add(e);
                                e = (Entity) e.Next;
                            }

                            // Load contents of the next room.
                            LoadRoom(nextRoom);
                        }
                        // Doesn't exist, clamp player.
                        else {
                            player.Entity.Position = new Point2(
                                Calc.Clamp(pos.X, bounds.X, bounds.X + bounds.Width),
                                Calc.Clamp(pos.Y, bounds.Y, bounds.Y + bounds.Height + 100)
                            );

                            // Reload if they fell out the bottom.
                            if (player.Entity.Position.Y > bounds.Y + bounds.Height + 64) {
                                world.Clear();
                                LoadRoom(Room, true);
                            }
                        }
                    }

                    // Death ... delete everything except the player
                    // then when they fall ut of the screen, we reset.
                    if (player.Health <= 0) {
                        Entity e = world.FirstEntity();

                        while (e != null) {
                            Entity next = (Entity) e.Next;

                            if (e.Get<Player>() == null)
                                world.DestroyEntity(e);

                            e = next;
                        }
                    }
                }
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
                        var player = world.First<Player>();

                        if (player != null)
                            player.Get<Mover>().Speed = new Vector2(0, -150);
                    }

                    // Delete old objects (except player!)
                    foreach (Entity it in lastEntities) {
                        if (it.Get<Player>() == null)
                            world.DestroyEntity(it);
                    }

                    Time.PauseFor(0.1f);
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

            // Hacky start / end screen text.
            if (Room == new Point2(0, 0) || lastRoom == new Point2(0, 0)) {
                var w = Content.Font.WidthOf(Title);
                var pos = new Point2((Width - (int) w) / 2, 20);
                Batch.Text(Content.Font, Title, pos + new Point2(0, 1), Color.Black);
                Batch.Text(Content.Font, Title, pos, Color.White);

                w = Content.Font.WidthOf(Controls);
                pos.X = (Width - (int) w) / 2;
                pos.Y += 20;
                Batch.Text(Content.Font, Controls, pos, Color.White * 0.25f);
            }
            else if (Room == new Point2(13, 0)) {
                var w = Content.Font.WidthOf(Ending);
                var pos = new Point2(Room.X * Width + Width / 2, Room.Y * Height + 20);
                Batch.Text(Content.Font, Ending, pos + new Point2(0, 1), Color.Black);
                Batch.Text(Content.Font, Ending, pos, Color.White);
            }

            // End camera offset.
            Batch.PopMatrix();

            // Draw the health.
            var player = world.First<Player>();

            if (player != null) {
                var hearts = Content.FindSprite("heart");
                var full = hearts.GetAnimation("full");
                var empty = hearts.GetAnimation("empty");

                Point2 pos = new Point2(0, Height - 16);
                Batch.Rect(new Rect(pos.X, pos.Y + 7, 40, 4), Color.Black);

                for (int i = 0; i < Player.MAX_HEALTH; i++) {
                    if (player.Health >= i + 1)
                        Batch.Image(full.Frames[0].Image, pos, Color.White);
                    else
                        Batch.Image(empty.Frames[0].Image, pos, Color.White);

                    pos.X += 12;
                }
            }

            // Draw FPS.
            if (drawColliders) {
                var fpsText = "FPS: " + Time.FPS;
                var w = Content.Font.WidthOf(fpsText);
                var pos = new Point2((Width - (int)w) - 5, 5);
                Batch.Text(Content.Font, fpsText, pos, Color.White);
            }

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
