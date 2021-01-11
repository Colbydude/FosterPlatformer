using Foster.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FosterPlatformer
{
    public class World
    {
        public static int MaxComponentTypes = 256;

        private Pool<Entity> cache = new Pool<Entity>();
        private Pool<Entity> alive = new Pool<Entity>();
        private Dictionary<Type, Pool<Component>> componentsCache = new Dictionary<Type, Pool<Component>>();
        private Dictionary<Type, Pool<Component>> componentsAlive = new Dictionary<Type, Pool<Component>>();
        private List<Component> visible = new List<Component>();

        // NOTE:
        // I tossed this reference here at the very end of making the game,
        // just so that the boss could tell the game to shake the screen.
        // Ideally I think there should be a Camera component that handles
        // that instead.
        public Game game;

        ~World()
        {
            // Destroy all the entities.
            while (alive.First != null)
                DestroyEntity(alive.First);

            // Delete component instances.
            componentsCache.Clear();

            // Delete entity instances.
            cache = null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        public Entity AddEntity(Point2? point)
        {
            Point2 position = point ?? new Point2(0, 0);

            // Create entity instance.
            Entity instance;

            if (cache.First != null) {
                instance = cache.First;
                cache.Remove(instance);
            } else {
                instance = new Entity();
            }

            // Add to list.
            alive.Insert(instance);

            // Assign.
            instance.Position = position;
            instance.World = this;

            // Return new entity!
            return instance;
        }

        /// <summary>
        ///
        /// </summary>
        public Entity FirstEntity()
        {
            return alive.First;
        }

        /// <summary>
        ///
        /// </summary>
        public Entity LastEntity()
        {
            return alive.Last;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entity"></param>
        public void DestroyEntity(Entity entity)
        {
            if (entity != null && entity.World == this) {
                // Destroy components.
                foreach (Component it in entity.Components)
                    Destroy(it);

                // Remove ourselves from the list.
                alive.Remove(entity);
                cache.Insert(entity);

                // Donezo.
                entity.World = null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <typeparam name="T"></typeparam>
        public T Add<T>(Entity entity, T component) where T : Component
        {
            Debug.Assert(entity != null, "Entity cannot be null.");
            Debug.Assert(entity.World == this, "Entity must be part of this world.");

            // Get the component type.
            Type type = typeof(T);
            var cache = componentsCache.GetValueOrDefault(type, null);
            var alive = componentsAlive.GetValueOrDefault(type, null);

            if (cache == null) {
                componentsCache[type] = cache = new Pool<Component>();
            }

            if (alive == null) {
                componentsAlive[type] = alive = new Pool<Component>();
            }

            // Instantiate a new instance.
            T instance;
            if (cache.First != null) {
                instance = (T) cache.First;
                cache.Remove(instance);
            }

            // Construct the new instance.
            instance = component;
            instance.Entity = entity;

            // Add it into the live components.
            alive.Insert(instance);

            // Add it to the entity.
            entity.Components.Add(instance);

            // and we're done!
            instance.Awake();

            return instance;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T First<T>() where T : Component
        {
            Type type = typeof(T);
            Pool<Component> pool = componentsAlive.GetValueOrDefault(type, null);

            if (pool == null)
                return null;

            return (T) pool.First;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Last<T>() where T : Component
        {
            Type type = typeof(T);
            Pool<Component> pool = componentsAlive.GetValueOrDefault(type, null);

            if (pool == null)
                return null;

            return (T) pool.Last;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="component"></param>
        public void Destroy(Component component)
        {
            if (component != null && component.Entity != null && component.Entity.World == this) {
                Type type = component.GetType();

                // Mark destroyed.
                component.Destroyed();

                // Remove from entity.
                var list = component.Entity.Components;
                foreach (Component it in list) {
                    if (it == component) {
                        list.Remove(it);
                        break;
                    }
                }

                // Remove from list.
                componentsAlive[type].Remove(component);
                componentsCache[type].Remove(component);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Clear()
        {
            Entity entity = FirstEntity();

            while (entity != null) {
                var next = entity.Next;
                DestroyEntity(entity);
                entity = (Entity) next;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Update()
        {
            foreach (var pool in componentsAlive) {
                var component = pool.Value.First;

                while (component != null) {
                    var next = component.Next;

                    if (component.Active && component.Entity.Active)
                        component.Update();

                    component = (Component) next;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="batch"></param>
        public void Render(Batch2D batch)
        {
            // Notes:
            // In general this isn't a great way to render objects.
            // Every frame it has to rebuild the list and sort it.
            // A more ideal way would be to cache the visible list
            // and insert / remove objects as they update or change
            // their depth

            // However, given the scope of this project, this is fine.

            // Assemble list.
            foreach (var pool in componentsAlive) {
                var component = pool.Value.First;

                while (component != null) {
                    if (component.Visible && component.Entity.Visible)
                        visible.Add(component);

                    component = (Component) component.Next;
                }
            }

            // Sort by depth.
            visible.Sort((Component a, Component b) => {
                if (a.Depth > b.Depth) return 1;
                else if (a.Depth < b.Depth) return -1;
                return 0;
            });

            // Render them.
            foreach (var it in visible)
                it.Render(batch);

            // Clear list for the next time around.
            visible.Clear();
        }
    }
}
