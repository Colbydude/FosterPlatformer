using Foster.Framework;
using System;

namespace FosterPlatformer.Components
{
    // Automatically checks if the provided collider ever overlaps
    // with something in the `hurt_by` mask. Makes it easy for enemies
    // to check if they were hit by `Mask.PlayerAttack`
    public class Hurtable : Component
    {
        public delegate void HurtEvent(Hurtable self);

        public Collider Collider = null;
        public float FlickerTimer = 0;
        public int HurtBy = 0;
        public float StunTimer = 0;
        public event HurtEvent OnHurt;

        public void Hurt()
        {
            Time.PauseFor(0.1f);
            StunTimer = 0.5f;
            FlickerTimer = 0.5f;
            OnHurt(this);
        }

        public override void Update()
        {
            if (Collider != null && OnHurt != null && StunTimer <= 0) {
                if (Collider.Check(HurtBy, new Point2(0, 0)))
                    Hurt();
            }

            StunTimer -= Time.Delta;

            if (FlickerTimer > 0) {
                FlickerTimer -= Time.Delta;

                if (Time.OnInterval(0.05f))
                    Entity.Visible = !Entity.Visible;
                if (FlickerTimer <= 0)
                    Entity.Visible = true;
            }
        }
    }
}
