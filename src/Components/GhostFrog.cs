using Foster.Framework;
using System;

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
                // @TODO
            }
            // FLOATING STATE
            else if (state == ST_FLOATING) {
                // @TODO
            }
            // SHOOTING STATE
            else if (state == ST_SHOOT) {
                // @TODO
            }
            // REFLECT STATE
            else if (state == ST_REFLECT) {
                // @TODO
            }
            // DEAD STATE
            else if (state == ST_DEAD_STATE) {
                // @TODO
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
                    // Time.PauseFor(0.25f);
                    SetState(ST_READYING_ATTACK);
                }
            }
        }

        public void SetState(int state)
        {
            this.state = state;
            timer = 0;
        }
    }
}
