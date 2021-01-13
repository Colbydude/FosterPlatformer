using Foster.Framework;
using System;

namespace FosterPlatformer.Components
{
    public delegate void End(Timer self);

    public class Timer : Component
    {
        public event End OnEnd;

        private float duration = 0;

        public Timer(float duration, End onEnd)
        {
            this.duration = duration;
            OnEnd = onEnd;
        }

        public void Start(float duration)
        {
            this.duration = duration;
        }

        public override void Update()
        {
            if (duration > 0) {
                duration -= Time.Delta;

                if (duration <= 0 && OnEnd != null) {
                    OnEnd(this);
                }
            }
        }
    }
}
