using Foster.Framework;
using Foster.GLFW;
using Foster.OpenGL;
using System;

namespace FosterGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Our System Module (this is Mandatory).
            App.Modules.Register<GLFW_System>();

            // Our Graphics Module (not Mandatory but required for drawing anything).
            App.Modules.Register<GL_Graphics>();

            // Register our Custom Module, where we will run our own code.
            App.Modules.Register<CustomModule>();

            // Start the Application with a single 1280x720 Window.
            App.Start("FosterPlatformer", 1280, 720);
        }
    }

    public class CustomModule : Module
    {
        public readonly Batch2D Batcher = new Batch2D();
        public float Offset = 0f;

        // This is called when the Application has Started.
        protected override void Startup()
        {
            // Add a Callback to the Primary Window's Render loop.
            // By Default a single Window is created at startup
            // Alternatively App.System.Windows has a list of all open windows.
            App.Window.OnRender += Render;
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
            Offset += 32 * Time.Delta;

            if (Offset > App.Window.Width) {
                Offset = 0;
            }
        }

        private void Render(Window window)
        {
            // Clear the Window.
            App.Graphics.Clear(window, Color.Black);

            // Clear the Batcher's data from the previous frame.
            // If you don't clear it, further calls will be added
            // to the batcher, which will eventually create huge
            // amounts of data.
            Batcher.Clear();

            // Draw a rectangle.
            Batcher.Rect(Offset, 0, 32, 32, Color.Red);

            // Draw the batcher to the Window.
            Batcher.Render(window);
        }
    }
}
