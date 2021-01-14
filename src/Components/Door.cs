using Foster.Framework;
using System;

namespace FosterPlatformer.Components
{
    public class Door : Component
    {
        public bool Waiting = false;

        public override void Update()
        {
            // Check if all enemies are dead.
            if (World().First<Enemy>() == null) {
                Factory.Pop(World(), Entity.Position + new Point2(0, -8));
                Entity.Destroy();
            }
        }

        public void Lock(Timer timer)
        {
            timer.Start(0.25f);

            if (Waiting) {
                var player = timer.World().First<Player>();

                if (player.Entity.Position.X > Entity.Position.X + 12) {
                    Factory.MakeDoorContents(Entity);
                    Factory.Pop(World(), Entity.Position + new Point2(0, -8));
                    Waiting = false;
                }
                else
                    return;
            }
        }
    }
}
