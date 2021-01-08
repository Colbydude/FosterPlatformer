using Foster.Framework;
using Foster.GLFW;
using Foster.OpenGL;
using System;

namespace FosterPlatformer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Our System Module (this is Mandatory).
            App.Modules.Register<GLFW_System>();

            // Our Graphics Module (not Mandatory but required for drawing anything).
            App.Modules.Register<GL_Graphics>();

            // Register our Game Module, where we will run our own code.
            App.Modules.Register<Game>();

            // Start the Application with a single 1280x720 Window.
            App.Start("Sword II: Adventure of Frog (Foster Port)", 1280, 720);
        }
    }
}
