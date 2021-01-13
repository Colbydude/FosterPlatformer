using Foster.Framework;
using System;
using System.Numerics;

namespace FosterPlatformer.Components
{
    public class Mover : Component
    {
        public delegate void HitXEvent(Mover mover);
        public delegate void HitYEvent(Mover mover);

        public Collider Collider = null;
        public Vector2 Speed;
        public float Gravity = 0;
        public float Friction = 0;
        public event HitXEvent OnHitX;
        public event HitYEvent OnHitY;

        private Vector2 remainder;

        public bool MoveX(int amount)
        {
            if (Collider != null) {
                int sign = Math.Sign(amount);

                while (amount != 0) {
                    if (Collider.Check(Mask.Solid, new Point2(sign, 0))) {
                        if (OnHitX != null)
                            OnHitX(this);
                        else
                            StopX();

                        return true;
                    }

                    amount -= sign;
                    Entity.Position.X += sign;
                }
            }
            else {
                Entity.Position.X += amount;
            }

            return false;
        }

        public bool MoveY(int amount)
        {
            if (Collider != null) {
                int sign = Math.Sign(amount);

                while (amount != 0) {
                    // If hit solid.
                    bool hitSomething = Collider.Check(Mask.Solid, new Point2(0, sign));

                    // No solid, but we're moving down, check for jumpthru
                    // but ONLY if we're not already overlapping a jumpthru.
                    if (!hitSomething && sign > 0)
                        hitSomething = Collider.Check(Mask.Jumpthru, new Point2(0, sign)) && !Collider.Check(Mask.Jumpthru, new Point2(0, 0));

                    // Stop movement.
                    if (hitSomething) {
                        if (OnHitY != null)
                            OnHitY(this);
                        else
                            StopY();

                        return true;
                    }

                    amount -= sign;
                    Entity.Position.Y += sign;
                }
            }
            else {
                Entity.Position.Y += amount;
            }

            return false;
        }

        public void StopX()
        {
            Speed.X = 0;
            remainder.X = 0;
        }

        public void StopY()
        {
            Speed.Y = 0;
            remainder.Y = 0;
        }

        public void Stop()
        {
            Speed.X = 0;
            Speed.Y = 0;
            remainder.X = 0;
            remainder.Y = 0;
        }

        public bool OnGround(int dist = 1)
        {
            if (Collider == null)
                return false;

            return
                // Solid
                Collider.Check(Mask.Solid, new Point2(0, dist)) ||
                // Jumpthru
                (Collider.Check(Mask.Jumpthru, new Point2(0, dist)) && !Collider.Check(Mask.Jumpthru, new Point2(0, 0)));
        }

        public override void Update()
        {
            // Apply friction maybe.
            if (Friction > 0 && OnGround())
                Speed.X = Calc.Approach(Speed.X, 0, Friction * Time.Delta);

            // Apply gravity.
            if (Gravity != 0 && (Collider == null || !Collider.Check(Mask.Solid, new Point2(0, 1))))
                Speed.Y += Gravity * Time.Delta;

            // Get the amount we should move, including remainder from the previous frame.
            Vector2 total = remainder + Speed * Time.Delta;

            // Round to integer values since we only move in pixels at a time.
            Point2 toMove = new Point2((int) total.X, (int) total.Y);

            // Store remainder floating values for next frame.
            remainder.X = total.X - toMove.X;
            remainder.Y = total.Y - toMove.Y;

            // Move by integer values.
            MoveX(toMove.X);
            MoveY(toMove.Y);
        }
    }
}
