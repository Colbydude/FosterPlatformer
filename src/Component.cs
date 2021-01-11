using Foster.Framework;
using System;
using System.Diagnostics;

namespace FosterPlatformer
{
    public class Component : Poolable
    {
        public bool Active = true;
        public bool Visible = true;
        public int Depth = 0;
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
