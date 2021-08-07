using Foster.Framework;
using System;

namespace FosterPlatformer.Components
{
    public class Mosquito : Component
    {
        public int Health = 2;
        public float timer = 0;

        public override void Update()
        {
            var mover = Get<Mover>();
            var player = World().First<Player>();

            if (player != null) {
                var diff = player.Entity.Position.X - Entity.Position.X;
                var dist = Math.Abs(diff);

                if (dist < 100)
                    mover.Speed.X += Math.Sign(diff) * 100 * Time.Delta;
                else
                    mover.Speed.X = Calc.Approach(mover.Speed.X, 0, 100 * Time.Delta);

                if (Math.Abs(mover.Speed.X) > 50)
                    mover.Speed.X = Calc.Approach(mover.Speed.X, Math.Sign(mover.Speed.X) * 50, 800 * Time.Delta);

                mover.Speed.Y = (int) (Math.Sin(timer) * 10);
            }

            timer += Time.Delta * 4;
        }

        public void Hurt()
        {
            Health--;

            if (Health <= 0) {
                Factory.Pop(World(), Entity.Position);
                Entity.Destroy();
            }
            else {
                var mover = Get<Mover>();
                var player = World().First<Player>();
                var sign = Math.Sign(player.Entity.Position.X - Entity.Position.X);
                mover.Speed.X = -sign * 140;
            }
        }
    }
}
