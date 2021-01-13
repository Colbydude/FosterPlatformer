using Foster.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FosterPlatformer
{
    public class Component : Poolable
    {
        public static class Types
        {
            public static Dictionary<Type, int> TypeMap = new Dictionary<Type, int>();
            private static int counter = 0;

            public static int Count()
            {
                return TypeMap.Count;
            }

            public static int Id<T>()
            {
                if (TypeMap.ContainsKey(typeof(T)))
                {
                    return TypeMap[typeof(T)];
                }

                TypeMap.Add(typeof(T), counter);
                Log.Message(String.Format("{0} mapped to {1}", typeof(T), counter));

                return counter++;
            }
        }

        public bool Active = true;
        public bool Visible = true;
        public int Depth = 0;
        public int Type = 0;
        public Entity Entity = null;

        /// <summary>
        /// Get the component of type T off of the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T Get<T>() where T : Component
        {
            Debug.Assert(Entity != null, "Component must be assigned to an Entity.");

            return Entity.Get<T>();
        }

        /// <summary>
        ///
        /// </summary>
        public void Destroy()
        {
            if (Entity != null && Entity.World != null) {
                Entity.World.Destroy(this);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public World World()
        {
            return Entity != null ? Entity.World : null;
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Awake() {}

        /// <summary>
        ///
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        ///
        /// </summary>
        /// <param name="batch"></param>
        public virtual void Render(Batch2D batch) {}

        /// <summary>
        ///
        /// </summary>
        public virtual void Destroyed() {}
    }
}
