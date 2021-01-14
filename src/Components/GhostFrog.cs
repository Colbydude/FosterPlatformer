using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class GhostFrog : Component
    {
        public static readonly int ST_WAITING = 0;
        public static readonly int ST_READYING_ATTACK = 1;
        public static readonly int ST_PERFORM_SLASH = 2;
        public static readonly int ST_FLOATING = 3;
        public static readonly int ST_SHOOT = 4;
        public static readonly int ST_REFLECT = 5;
        public static readonly int ST_DEAD_STATE = 6;

        // Health during our first phase.
        public static readonly int MAX_HEALTH_1 = 10;

        // Health during our second phase.
        public static readonly int MAX_HEALTH_2 = 3;

        // Current health value (assigned to phase 1 health to start)
        public int Health = MAX_HEALTH_1;

        // Phase 0 or 1
        //      0 = running along the ground and slicing
        //      1 = flying around in the air shooting orbs
        public int Phase = 0;

        private float timer = 0;
        private int state = ST_WAITING;
        private int facing = 1;
        private int side = 1;
        private int reflectCount = 0;
        private Point2 home;
        private Point2 lastPos;
        private Collider attackCollider = null;

        public override void Awake()
        {
            home = Entity.Position;
        }

        public override void Update()
        {
            timer += Time.Delta;

            var player = World().First<Player>();
            var mover = Get<Mover>();
            var anim = Get<Animator>();
            var hitbox = Get<Collider>();

            // No player - turn off AI.
            if (player == null)
                return;

            var x = Entity.Position.X;
            var y = Entity.Position.Y;
            var playerX = player.Entity.Position.X;

            // Flip sprite.
            anim.Scale = new System.Numerics.Vector2(facing, 1);

            // NORMAL STATE
            if (state == ST_READYING_ATTACK) {
                facing = Math.Sign(playerX - x);

                if (facing == 0)
                    facing = 1;

                float targetX = playerX + 32 * -facing;
                mover.Speed.X = Calc.Approach(mover.Speed.X, Math.Sign(targetX - x) * 40, 400 * Time.Delta);
                mover.Friction = 100;
                anim.Play("run");

                if (timer > 3.0f ||
                    (timer > 1.0f && hitbox.Check(Mask.Solid, new Point2(-facing * 8, 0)))) {
                    mover.Speed.X = 0;
                    SetState(ST_PERFORM_SLASH);
                }
            }
            // SLASH STATE
            else if (state == ST_PERFORM_SLASH) {
                // Start attack anim.
                anim.Play("attack");
                mover.Friction = 500;

                // After 0.8s, do the lunge.
                if (Time.OnTime(timer, 0.8f)) {
                    mover.Speed.X = facing * 250;
                    hitbox.SetRect(new RectInt(-4 + facing * 4, -12, 8, 12));

                    RectInt rect = new RectInt(8, -8, 20, 8);
                    if (facing < 0)
                        rect.X = -(rect.X + rect.Width);

                    if (attackCollider != null)
                        attackCollider.Destroy();

                    attackCollider = Entity.Add<Collider>(Collider.MakeRect(rect));
                    attackCollider.Mask = Mask.Enemy;
                }
                // Turn off attack collider.
                else if (Time.OnTime(timer, anim.Animation().Duration() - 1.0f)) {
                    if (attackCollider != null)
                        attackCollider.Destroy();

                    attackCollider = null;
                }
                // End attack state.
                else if (timer >= anim.Animation().Duration()) {
                    hitbox.SetRect(new RectInt(-4, -12, 8, 12));

                    if (Health > 0) {
                        SetState(ST_READYING_ATTACK);
                    }
                    else {
                        Phase = 1;
                        Health = MAX_HEALTH_2;
                        side = Rand.Instance.Next(0, 2) == 0 ? -1 : 1;
                        SetState(ST_FLOATING);
                    }
                }
            }
            // FLOATING STATE
            else if (state == ST_FLOATING) {
                anim.Play("float");

                mover.Friction = 0;
                mover.Collider = null;

                float targetY = home.Y - 50;
                float targetX = home.X + side * 50;

                if (Math.Sign(targetY - y) != Math.Sign(targetY - lastPos.Y)) {
                    mover.Speed.Y = 0;
                    Entity.Position.Y = (int) targetY;
                }
                else
                    mover.Speed.Y = Calc.Approach(mover.Speed.Y, Math.Sign(targetY - y) * 50, 800 * Time.Delta);

                if (Math.Abs(y - targetY) < 8)
                    mover.Speed.X = Calc.Approach(mover.Speed.X, Math.Sign(targetX - x) * 80, 800 * Time.Delta);
                else
                    mover.Speed.X = 0;

                if (timer > 5.0f || (Math.Abs(targetX - x) < 8 && Math.Abs(targetY - y) < 8))
                    SetState(ST_SHOOT);
            }
            // SHOOTING STATE
            else if (state == ST_SHOOT) {
                mover.Speed = Calc.Approach(mover.Speed, Vector2.Zero, 300 * Time.Delta);

                facing = Math.Sign(playerX - x);
                if (facing == 0)
                    facing = 1;

                if (Time.OnTime(timer, 1.0f)) {
                    anim.Play("reflect");
                }
                else if (Time.OnTime(timer, 1.2f)) {
                    Factory.Orb(World(), Entity.Position + new Point2(facing * 12, -8));
                    reflectCount = 0;
                }
                else if (Time.OnTime(timer, 1.4f)) {
                    anim.Play("float");
                    SetState(ST_REFLECT);
                }
            }
            // REFLECT STATE
            else if (state == ST_REFLECT) {
                if (Time.OnTime(timer, 0.4f))
                    anim.Play("float");

                var orb = World().First<Orb>();
                if (orb == null) {
                    if (timer > 1.0f) {
                        side = -side;
                        SetState(ST_FLOATING);
                    }
                }
                else if (!orb.TowardsPlayer) {
                    float distance = new Vector2(
                        orb.Entity.Position.X - orb.Target().X,
                        orb.Entity.Position.Y - orb.Target().Y
                    ).Length();

                    if (reflectCount < 2) {
                        if (distance < 16) {
                            var sign = Math.Sign(orb.Entity.Position.X - x);
                            if (sign != 0)
                                facing = sign;

                            anim.Play("reflect");
                            orb.OnHit();

                            reflectCount++;
                            timer = 0;
                        }
                    }
                    else {
                        if (distance < 8) {
                            Factory.Pop(World(), Entity.Position + new Point2(0, -8));
                            orb.Entity.Destroy();
                            Get<Hurtable>().Hurt();
                            timer = 0;
                        }
                    }
                }
            }
            // DEAD STATE
            else if (state == ST_DEAD_STATE) {
                anim.Play("dead");
                World().Game.Shake(1.0f);

                if (Time.OnInterval(0.25f)) {
                    var offset = new Point2(Rand.Instance.Next(-16, 16), Rand.Instance.Next(-16, 16));
                    Factory.Pop(World(), Entity.Position + new Point2(0, -8) + offset);
                }

                if (Time.OnTime(timer, 3.0f)) {
                    for (int popx = -1; x < 2; x++)
                        for (int popy = -1; y < 2; y++)
                            Factory.Pop(World(), Entity.Position + new Point2(popx * 12, -8 + popy * 12));

                    Time.PauseFor(0.3f);
                    World().Game.Shake(0.1f);
                    Entity.Destroy();
                }
            }

            if (state == ST_FLOATING || state == ST_SHOOT || state == ST_REFLECT) {
                anim.Offset.Y = (int) Math.Sin(Time.FixedDuration.TotalMilliseconds * 2f) * 3; // @TODO Revisit?
            }

            lastPos = Entity.Position;
        }

        public void OnHurt(Hurtable hurtable)
        {
            if (Health > 0) {
                Health--;

                if (Health <= 0 && Phase > 0) {
                    SetState(ST_DEAD_STATE);
                }

                if (state == ST_WAITING) {
                    Factory.Pop(World(), Entity.Position + new Point2(0, -8));
                    Time.PauseFor(0.25f);
                    SetState(ST_READYING_ATTACK);
                }
            }
        }

        public void OnHitX(Mover self)
        {
            //
        }

        public void OnHitY(Mover self)
        {
            //
        }

        public void SetState(int state)
        {
            this.state = state;
            timer = 0;
        }
    }
}
