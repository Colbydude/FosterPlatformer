using System;

namespace FosterPlatformer
{
    public struct Mask
    {
        public static int Solid = 1 << 0;
        public static int Jumpthru = 1 << 1;
        public static int PlayerAttack = 1 << 2;
        public static int Enemy = 1 << 3;
        public static int Player = 1 << 4;
    }
}
