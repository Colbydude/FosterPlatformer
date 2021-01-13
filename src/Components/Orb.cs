using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class Orb : Component
    {
        public float Speed = 40;
        public bool TowardsPlayer = true;

        public Point2 Target()
        {
            var player = World().First<Player>();
            var ghost = World().First<GhostFrog>();

            if (player != null && ghost != null)
                return (TowardsPlayer ? player.Entity.Position : ghost.Entity.Position) + new Point2(0, -8);

            return new Point2(0, 0);
        }

        public override void Update()
        {
            var mover = Get<Mover>();
            var diff = Target() - Entity.Position;
            var vecDiff = new Vector2(diff.X, diff.Y).Normalized();
            mover.Speed = vecDiff * Speed;
        }

        public override void Destroyed()
        {
            Factory.Pop(World(), Entity.Position);
        }

        public void OnHit()
        {
            TowardsPlayer = !TowardsPlayer;
            Speed += 40;

            var hurt = Get<Hurtable>();
            if (TowardsPlayer)
                hurt.StunTimer = 0;
        }
    }
}
