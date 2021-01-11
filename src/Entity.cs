using Foster.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FosterPlatformer
{
    public class Entity : Poolable
    {
        public bool Active = true;
        public bool Visible = true;
        public Point2 Position;
        public World World;
        public List<Component> Components = new List<Component>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="component"></param>
        /// <typeparam name="T"></typeparam>
        public T Add<T>(T component) where T : Component
        {
            Debug.Assert(World != null, "Entity must be assigned to a World.");

            return World.Add(this, component);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T Get<T>() where T : Component
        {
            Debug.Assert(World != null, "Entity must be assigned to a World.");

            foreach (var it in Components)
                if (it.GetType() == typeof(T))
                    return (T) it;

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        public void Destroy()
        {
            World.DestroyEntity(this);
        }
    }
}
