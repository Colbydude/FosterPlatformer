using Foster.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FosterPlatformer
{
    public class World
    {
        public static int MaxComponentTypes = 256;

        private Pool<Entity> alive = new Pool<Entity>();
        private Pool<Component>[] componentsAlive = new Pool<Component>[MaxComponentTypes];
        private List<Component> visible = new List<Component>();

        // NOTE:
        // I tossed this reference here at the very end of making the game,
        // just so that the boss could tell the game to shake the screen.
        // Ideally I think there should be a Camera component that handles
        // that instead.
        public Game Game;

        ~World()
        {
            // Destroy all the entities.
            while (alive.First != null)
                DestroyEntity(alive.First);
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

            instance = new Entity();

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
                for (int i = entity.Components.Count - 1; i >= 0; i--)
                    Destroy(entity.Components[i]);

                // Remove ourselves from the list.
                alive.Remove(entity);

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
            int typeId = Component.Types.Id<T>();
            var alive = componentsAlive[typeId];

            if (alive == null) {
                componentsAlive[typeId] = alive = new Pool<Component>();
            }

            component.Type = typeId;
            component.Entity = entity;

            // Add it into the live components.
            alive.Insert(component);

            // Add it to the entity.
            entity.Components.Add(component);

            // and we're done!
            component.Awake();

            return component;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T First<T>() where T : Component
        {
            int typeId = Component.Types.Id<T>();
            return (T) componentsAlive[typeId]?.First;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Last<T>() where T : Component
        {
            int typeId = Component.Types.Id<T>();
            return (T) componentsAlive[typeId]?.Last;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="component"></param>
        public void Destroy(Component component)
        {
            if (component != null && component.Entity != null && component.Entity.World == this) {
                int typeId = component.Type;

                // Mark destroyed.
                component.Destroyed();

                // Remove from entity.
                var list = component.Entity.Components;
                for (int i = list.Count - 1; i >= 0; i--) {
                    if (list[i] == component) {
                        list.Remove(list[i]);
                        break;
                    }
                }

                // Remove from list.
                componentsAlive[typeId].Remove(component);
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
            for (int i = 0; i < Component.Types.Count(); i++) {
                var component = componentsAlive[i]?.First;

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
            for (int i = 0; i < Component.Types.Count(); i++) {
                var component = componentsAlive[i]?.First;

                while (component != null) {
                    if (component.Visible && component.Entity.Visible)
                        visible.Add(component);

                    component = (Component) component.Next;
                }
            }

            // Sort by depth.
            visible.Sort((Component a, Component b) => {
                if (a.Depth > b.Depth) return -1;
                else if (a.Depth < b.Depth) return 1;
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
