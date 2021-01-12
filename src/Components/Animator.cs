using Foster.Framework;
using FosterPlatformer.Assets;
using FosterPlatformer.Extensions;
using System;
using System.Diagnostics;
using System.Numerics;

namespace FosterPlatformer
{
    public class Animator : Component
    {
        public Vector2 Scale = Vector2.One;
        public Point2 Offset = Point2.Zero;

        private Sprite sprite = null;
        private int animationIndex = 0;
        private int frameIndex = 0;
        private float frameCounter = 0;

        public Animator(string sprite)
        {
            this.sprite = Content.FindSprite(sprite);
            this.animationIndex = 0;
        }

        public Sprite Sprite()
        {
            return sprite;
        }

        public Sprite.Animation Animation()
        {
            if (sprite != null && animationIndex >= 0 && animationIndex < sprite.Animations.Count)
                return sprite.Animations[animationIndex];

            return null;
        }

        public void Play(string animation, bool restart = false)
        {
            Debug.Assert(sprite != null, "No Sprite assigned!");

            for (int i = 0; i < sprite.Animations.Count; i++) {
                if (sprite.Animations[i].Name == animation) {
                    if (animationIndex != i || restart) {
                        animationIndex = i;
                        frameIndex = 0;
                        frameCounter = 0;
                    }

                    break;
                }
            }
        }

        public override void Update()
        {
            // Only update if we're in a valid state.
            if (inValidState()) {
                // Quick references.
                var anim = sprite.Animations[animationIndex];
                var frame = anim.Frames[frameIndex];

                // Increment frame counter.
                frameCounter += Time.Delta;

                // Move to next frame after duration.
                while (frameCounter >= frame.Duration) {
                    // Reset frame counter.
                    frameCounter -= frame.Duration;

                    // Increment frame, move back if we're at the end.
                    frameIndex++;
                    if (frameIndex >= anim.Frames.Count)
                        frameIndex = 0;
                }
            }
        }

        public override void Render(Batch2D batch)
        {
            if (inValidState()) {
                batch.PushMatrix(
                    Mat3x2Ext.CreateTransform(Entity.Position + Offset, sprite.Origin, Scale, 0)
                );

                var anim = sprite.Animations[animationIndex];
                var frame = anim.Frames[frameIndex];
                batch.Image(frame.Image, Vector2.Zero, Color.White);

                batch.PopMatrix();
            }
        }

        private bool inValidState()
        {
            return
                sprite != null &&
                animationIndex >= 0 &&
                animationIndex < sprite.Animations.Count &&
                frameIndex >= 0 &&
                frameIndex < sprite.Animations[animationIndex].Frames.Count;
        }
    }
}
