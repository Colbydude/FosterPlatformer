using Foster.Framework;
using FosterPlatformer.Components;
using System;

namespace FosterPlatformer
{
    public class Factory
    {
        public static Entity Player(World world, Point2 position)
        {
            var en = world.AddEntity(position);

            var anim = en.Add<Animator>(new Animator("player"));
            anim.Play("idle");
            anim.Depth = -10;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-4, -12, 8, 12)));
            hitbox.Mask = Mask.Player;

            // var mover = en.Add<Mover>(new Mover());
            // mover.Collider = hitbox;

            en.Add<Player>(new Player());
            return en;
        }
    }
}
