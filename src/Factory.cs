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

            var mover = en.Add<Mover>(new Mover());
            mover.Collider = hitbox;

            en.Add<Player>(new Player());
            return en;
        }

        public static Entity Bramble(World world, Point2 position)
        {
            var en = world.AddEntity(position);

            var anim = en.Add<Animator>(new Animator("bramble"));
            anim.Play("idle");
            anim.Depth = -5;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-4, -8, 8, 8)));
            hitbox.Mask = Mask.Enemy;

            var hurtable = en.Add<Hurtable>(new Hurtable());
            hurtable.HurtBy = Mask.PlayerAttack;
            hurtable.Collider = hitbox;
            hurtable.OnHurt += (Hurtable self) => {
                Pop(self.World(), self.Entity.Position + new Point2(0, -4));
                self.Entity.Destroy();
            };

            return en;
        }

        public static Entity Pop(World world, Point2 position)
        {
            var en = world.AddEntity(position);

            var anim = en.Add<Animator>(new Animator("pop"));
            anim.Play("pop");
            anim.Depth = -20;

            var timer = en.Add<Timer>(new Timer(anim.Animation().Duration(), (Timer self) => {
                self.Entity.Destroy();
            }));

            return en;
        }

        // @TODO Spitter
        // @TODO Bullet
        // @TODO Mosquito
        // @TODO Door
        // @TODO Blob

        public static Entity GhostFrog(World world, Point2 position)
        {
            var en = world.AddEntity(position);
            en.Add<GhostFrog>(new GhostFrog());
            en.Add<Enemy>(new Enemy());

            var anim = en.Add<Animator>(new Animator("ghostfrog"));
            anim.Play("sword");
            anim.Depth = -5;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-4, -12, 8, 12)));
            hitbox.Mask = Mask.Enemy;

            var mover = en.Add<Mover>(new Mover());
            mover.Collider = hitbox;
            mover.Gravity = 0;
            mover.Friction = 100;
            mover.OnHitX += (Mover self) => { self.Get<GhostFrog>().OnHitX(self); };
            mover.OnHitY += (Mover self) => { self.Get<GhostFrog>().OnHitY(self); };

            var hurtable = en.Add<Hurtable>(new Hurtable());
            hurtable.HurtBy = Mask.PlayerAttack;
            hurtable.Collider = hitbox;
            hurtable.OnHurt += (Hurtable self) => { self.Get<GhostFrog>().OnHurt(self); };

            return en;
        }

        public static Entity Orb(World world, Point2 position)
        {
            var en = world.AddEntity(position);
            en.Add<Orb>(new Orb());

            var anim = en.Add<Animator>(new Animator("bullet"));
            anim.Play("idle");
            anim.Depth = -5;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-4, -4, 8, 8)));
            hitbox.Mask = Mask.Enemy;

            var mover = en.Add<Mover>(new Mover());
            mover.Collider = hitbox;
            mover.OnHitX += (Mover self) => { Factory.Pop(self.World(), self.Entity.Position); self.Entity.Destroy(); };
            mover.OnHitY += (Mover self) => { Factory.Pop(self.World(), self.Entity.Position); self.Entity.Destroy(); };

            var hurtable = en.Add<Hurtable>(new Hurtable());
            hurtable.HurtBy = Mask.PlayerAttack;
            hurtable.Collider = en.Add<Collider>(Collider.MakeRect(new RectInt(-8, -8, 16, 16)));
            hurtable.OnHurt += (Hurtable self) => { self.Get<Orb>().OnHit(); };

            return en;
        }
    }
}
