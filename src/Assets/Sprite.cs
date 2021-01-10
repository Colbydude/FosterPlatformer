using Foster.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace FosterPlatformer.Assets
{
    public class Sprite
    {
        public struct Frame
        {
            public Subtexture Image;
            public float Duration;
        }

        public struct Animation
        {
            public string Name;
            public List<Frame> Frames;

            public float Duration()
            {
                float d = 0;
                foreach (Frame it in Frames)
                    d += it.Duration;
                return d;
            }
        }

        public string Name;
        public Vector2 Origin;
        public List<Animation> Animations;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public Animation? GetAnimation(string name)
        {
            foreach (Animation it in Animations)
                if (it.Name == name)
                    return it;

            return null;
        }
    }
}
