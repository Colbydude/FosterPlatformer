using System;
using System.Numerics;

namespace FosterPlatformer.Extensions
{
    public static class Mat3x2Ext
    {
        /// <summary>
        ///
        /// </summary>
        public static Matrix3x2 CreateTransform(in Vector2 position, in Vector2 origin, in Vector2 scale, float rotation)
        {
            Matrix3x2 matrix = Matrix3x2.Identity;

            if (origin.X != 0 || origin.Y != 0)
                matrix = Matrix3x2.CreateTranslation(-origin.X, -origin.Y);
            if (scale.X != 1 || scale.Y != 1)
                matrix = matrix * Matrix3x2.CreateScale(scale);
            if (rotation != 0)
                matrix = matrix * Matrix3x2.CreateRotation(rotation);
            if (position.X != 0 || position.Y != 0)
                matrix = matrix * Matrix3x2.CreateTranslation(position);

            return matrix;
        }
    }
}
