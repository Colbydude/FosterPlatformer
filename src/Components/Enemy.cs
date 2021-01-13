using System;

namespace FosterPlatformer.Components
{
    // I just needed a way to track whether any enemies were still
    // in the scene or not, so Locked Doors could open. The easiest
    // way was to add an Enemy component, and check if there are any
    // in the scene.
    public class Enemy : Component {}
}
