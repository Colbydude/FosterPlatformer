using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class Player : Component
    {
        public static readonly int ST_NORMAL = 0;
        public static readonly int ST_ATTACK = 1;
        public static readonly int ST_HURT = 2;
        public static readonly int ST_START = 3;
        public static readonly int MAX_HEALTH = 4;

        public int Health = MAX_HEALTH;

        public VirtualStick InputMove;
        public VirtualButton InputJump;
        public VirtualButton InputAttack;

        private int state = ST_START;
        private int facing = 1;
        private float jumpTimer = 0;
        private float attackTimer = 0;
        private float hurtTimer = 0;
        private float invincibleTimer = 0;
        private float startTimer = 1;
        private Collider attackCollider = null;
        private bool onGround = false;

        private float maxGroundSpeed = 60;
        private float maxAirSpeed = 70;
        private float groundAccel = 500;
        private float airAccel = 20;
        private float friction = 800;
        private float hurtFriction = 200;
        private float gravity = 450;
        private float jumpTime = 0.18f;
        private float hurtDuration = 0.5f;
        private float invincibleDuration = 1.5f;

        public Player()
        {
            InputMove = new VirtualStick(App.Input)
                .Add(Keys.Left, Keys.Right, Keys.Up, Keys.Down)                 // Keyboard Arrow Keys
                .Add(0, Buttons.Left, Buttons.Right, Buttons.Up, Buttons.Down)  // Controller DPad
                .Add(0, Axes.LeftX, Axes.LeftY, 0.2f);                          // Controller Left Stick

            InputJump = new VirtualButton(App.Input, 0.15f)
                .Add(Keys.X)                                                    // Keyboard X
                .Add(0, Buttons.A);                                             // Controller A Button

            InputAttack = new VirtualButton(App.Input, 0.15f)
                .Add(Keys.C)                                                    // Keyboard C
                .Add(0, Buttons.X);                                             // Controller X Button
        }

        public override void Update()
        {
            var mover = Get<Mover>();
            var anim = Get<Animator>();
            var hitbox = Get<Collider>();
            var wasOnGround = onGround;
            onGround = mover.OnGround();
            int input = InputMove.IntValue.X;

            #region Sprite stuff

            // Land squish.
            if (!wasOnGround && onGround)
                anim.Scale = new Vector2(facing * 1.5f, 0.7f);

            // Lerp scale back to one.
            anim.Scale = Calc.Approach(anim.Scale, new Vector2(facing, 1.0f), Time.Delta * 4);

            // Set facing.
            anim.Scale.X = Math.Abs(anim.Scale.X) * facing;

            #endregion

            // START
            if (state == ST_START) {
                while (hitbox.Check(Mask.Solid, new Point2(0, 0)))
                    Entity.Position.Y++;

                anim.Play("sword");
                startTimer -= Time.Delta;

                if (startTimer <= 0)
                    state = ST_NORMAL;
            }
            // NORMAL
            else if (state == ST_NORMAL) {
                // Current animation.
                if (onGround) {
                    if (input != 0)
                        anim.Play("run");
                    else
                        anim.Play("idle");
                }
                else {
                    anim.Play("jump");
                }

                #region Horizontal Movement

                // Acceleration.
                mover.Speed.X += input * (onGround ? groundAccel : airAccel) * Time.Delta;

                // Maxspeed.
                var maxspd = (onGround ? maxGroundSpeed : maxAirSpeed);
                if (Math.Abs(mover.Speed.X) > maxspd) {
                    mover.Speed.X = Calc.Approach(
                        mover.Speed.X,
                        Math.Sign(mover.Speed.X) * maxspd,
                        2000 * Time.Delta
                    );
                }

                // Friction.
                if (input == 0 && onGround)
                    mover.Speed.X = Calc.Approach(mover.Speed.X, 0, friction * Time.Delta);

                // Facing Direction
                if (input != 0 && onGround)
                    facing = input;

                #endregion

                // Invoke Jumping.
                if (InputJump.Pressed && mover.OnGround()) {
                    // InputJump.ClearPressedBuffer(); ??
                    anim.Scale = new Vector2(facing * 0.65f, 1.4f);
                    mover.Speed.X = input * maxAirSpeed;
                    jumpTimer = jumpTime;
                }

                // Begin Attack.
                if (InputAttack.Pressed) {
                    // InputAttack.ClearPressBuffer(); ??

                    state = ST_ATTACK;
                    attackTimer = 0;

                    if (attackCollider == null)
                        attackCollider = Entity.Add<Collider>(Collider.MakeRect(new RectInt()));

                    attackCollider.Mask = Mask.PlayerAttack;

                    if (onGround)
                        mover.Speed.X = 0;
                }
            }
            // ATTACK
            else if (state == ST_ATTACK) {
                anim.Play("attack");
                attackTimer += Time.Delta;

                // Setup hitbox.
                if (attackTimer < 0.2f) {
                    attackCollider.SetRect(new RectInt(-16, -12, 16, 8));
                }
                else if (attackTimer < 0.5f) {
                    attackCollider.SetRect(new RectInt(8, -8, 16, 8));
                }
                else if (attackCollider != null) {
                    attackCollider.Destroy();
                    attackCollider = null;
                }

                // Flip hitbox if you're facing left.
                if (facing < 0 && attackCollider != null) {
                    var rect = attackCollider.GetRect();
                    rect.X = -(rect.X + rect.Width);
                    attackCollider.SetRect(rect);
                }

                // End the attack.
                if (attackTimer >= anim.Animation().Duration()) {
                    anim.Play("idle");
                    state = ST_NORMAL;
                }
            }
            else if (state == ST_HURT) {
                hurtTimer -= Time.Delta;

                if (hurtTimer <= 0)
                    state = ST_NORMAL;

                mover.Speed.X = Calc.Approach(mover.Speed.X, 0, hurtFriction * Time.Delta);
            }

            // Variable jumping.
            if (jumpTimer > 0) {
                mover.Speed.Y = -125;
                jumpTimer -= Time.Delta;

                if (InputJump.Down)
                    jumpTimer = 0;
            }

            // Invicible Timer.
            if (state != ST_HURT && invincibleTimer > 0) {
                if (Time.OnInterval(0.05f))
                    anim.Visible = !anim.Visible;

                invincibleTimer -= Time.Delta;

                if (invincibleTimer <= 0)
                    anim.Visible = true;
            }

            // Gravity
            if (!onGround) {
                float grav = gravity;

                if (state == ST_NORMAL && Math.Abs(mover.Speed.Y) < 20 && InputJump.Down)
                    grav *= 0.4f;

                mover.Speed.Y += grav * Time.Delta;
            }

            // Hurt Check!
            if (invincibleTimer <= 0) {
                var hit = hitbox.First(Mask.Enemy, new Point2(0, 0));

                if (hit != null) {
                    // Time.PauseFor(0.1f); ??
                    anim.Play("hurt");

                    if (attackCollider != null) {
                        attackCollider.Destroy();
                        attackCollider = null;
                    }

                    mover.Speed = new Vector2(-facing * 100, -80);

                    Health--;
                    hurtTimer = hurtDuration;
                    invincibleTimer = invincibleDuration;
                    state = ST_HURT;

                    // @TODO ORB
                }
            }
        }
    }
}
