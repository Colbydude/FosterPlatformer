using Foster.Framework;
using FosterPlatformer.Extensions;
using System;
using System.Numerics;

namespace FosterPlatformer
{
    public class Game : Module
    {
        public static readonly int Width = 240;
        public static readonly int Height = 135;
        public static readonly int TileWidth = 8;
        public static readonly int TileHeight = 8;
        public static readonly int Columns = Width / TileWidth;
        public static readonly int Rows = Height / TileHeight;

        public static readonly string Title = "SWORD II: ADVENTURE OF FROG";
        public static readonly string Controls = "arrow keys + X / C\nstick + A / X";
        public static readonly string Ending = "YOU SAVED POND\nAND YOU ARE\nA REAL HERO";

        // public World world;
        public FrameBuffer Buffer;
        public Batch2D Batch = new Batch2D();
        public Point2 Room;
        public Vector2 Camera;
        public bool Fullscreen = false;

        private bool drawColliders;
        private bool transition = false;
        private float nextEase;
        private Point2 nextRoom;
        private Point2 lastRoom;
        // Vector<Entity> lastEntities;
        private Point2 shake;
        private float shakeTimer = 0;

        private Point2 playerPosition = new Point2(0, 0);

        // This is called when the Application has Started.
        protected override void Startup()
        {
            // Add a Callback to the Primary Window's Render loop.
            // By Default a single Window is created at startup
            // Alternatively App.System.Windows has a list of all open windows.
            App.Window.OnRender += Render;

            // world.game = this;

            // Load content.
            // Content.Load();

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
            // @TODO
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

            // @TEMP
            if (App.Input.Keyboard.Down(Keys.Up))
                playerPosition.Y -= 1;
            if (App.Input.Keyboard.Down(Keys.Down))
                playerPosition.Y += 1;
            if (App.Input.Keyboard.Down(Keys.Left))
                playerPosition.X -= 1;
            if (App.Input.Keyboard.Down(Keys.Right))
                playerPosition.X += 1;

            // Normal Update
            if (!transition) {
                // @TODO
            }
            // Room Transition routine
            else {
                // @TODO
            }
        }

        private void Render(Window window)
        {
            #region Draw gameplay stuff.

            App.Graphics.Clear(Buffer, Color.FromHexStringRGB("#150e22"));

            // Push camera offset.
            Batch.PushMatrix(Matrix3x2.CreateTranslation(-Camera + shake));

            // Draw gameplay objects.
            // world.render(Batch);
            // @TEMP
            Batch.Rect(playerPosition.X, playerPosition.Y, 16, 16, Color.Red);

            // Draw debug colliders.
            if (drawColliders) {
                // @TODO
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
