using Foster.Framework;
using FosterPlatformer.Components;
using System;
using System.Numerics;

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

        public static Entity Spitter(World world, Point2 position)
        {
            var en = world.AddEntity(position);
            en.Add<Enemy>(new Enemy());

            var anim = en.Add<Animator>(new Animator("spitter"));
            anim.Play("idle");
            anim.Depth = -5;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-6, -12, 12, 12)));
            hitbox.Mask = Mask.Enemy;

            var hurtable = en.Add<Hurtable>(new Hurtable());
            hurtable.HurtBy = Mask.PlayerAttack;
            hurtable.Collider = hitbox;
            hurtable.OnHurt += (Hurtable self) => {
                Time.PauseFor(0.1f);
                Pop(self.World(), self.Entity.Position + new Point2(0, -4));
                self.Entity.Destroy();
            };

            var timer = en.Add<Timer>(new Timer(1.0f, (Timer self) => {
                Bullet(self.World(), self.Entity.Position + new Point2(-8, -8), -1);
                self.Get<Animator>().Play("shoot");
                self.Entity.Add<Timer>(new Timer(0.4f, (Timer self) => { self.Get<Animator>().Play("idle"); }));
                self.Start(3.0f);
            }));

            return en;
        }

        public static Entity Bullet(World world, Point2 position, int direction)
        {
            var en = world.AddEntity(position);

            var anim = en.Add<Animator>(new Animator("bullet"));
            anim.Play("idle");
            anim.Depth = -5;

            var hitbox = en.Add<Collider>(Collider.MakeRect(new RectInt(-4, -4, 8, 8)));
            hitbox.Mask = Mask.Enemy;

            var mover = en.Add<Mover>(new Mover());
            mover.Collider = hitbox;
            mover.Speed = new Vector2(direction * 40, 0);
            mover.Gravity = 130;
            mover.OnHitX += (Mover self) => { self.Entity.Destroy(); };
            mover.OnHitY += (Mover self) => { self.Speed.Y = -60; };

            var hurtable = en.Add<Hurtable>(new Hurtable());
            hurtable.HurtBy = Mask.PlayerAttack;
            hurtable.Collider = hitbox;
            hurtable.OnHurt += (Hurtable self) => {
                Time.PauseFor(0.1f);
                Pop(self.World(), self.Entity.Position + new Point2(0, -4));
                self.Entity.Destroy();
            };

            en.Add<Timer>(new Timer(2.5f, (Timer self) => {
                self.Get<Hurtable>().FlickerTimer = 100;
            }));

            en.Add<Timer>(new Timer(3.0f, (Timer self) => {
                self.Entity.Destroy();
            }));

            return en;
        }

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
