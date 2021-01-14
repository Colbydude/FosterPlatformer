using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class Blob : Component
    {
        public int Health = 3;

        public void Jump(Timer timer)
        {
            var mover = Get<Mover>();

            if (!mover.OnGround()) {
                timer.Start(0.05f);
            }
            else {
                Get<Animator>().Play("jump");
                timer.Start(2.0f);
                mover.Speed.Y = -90;

                var player = World().First<Player>();

                if (player != null) {
                    var dir = Math.Sign(player.Entity.Position.X - Entity.Position.X);
                    if (dir == 0) dir = 1;

                    Get<Animator>().Scale = new Vector2(dir, 1);
                    mover.Speed.X = dir * 40;
                }
            }
        }

        public void Hurt()
        {
            var player = World().First<Player>();

            if (player != null) {
                var mover = Get<Mover>();
                var sign = Math.Sign(Entity.Position.X - player.Entity.Position.X);
                mover.Speed.X = sign * 120;
            }

            Health--;
            if (Health <= 0) {
                Factory.Pop(World(), Entity.Position + new Point2(0, -4));
                Entity.Destroy();
            }
        }
    }
}
