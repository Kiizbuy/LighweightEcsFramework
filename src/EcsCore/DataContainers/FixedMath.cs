using System;

namespace EcsCore
{
    public static class FixedMath
    {
        public static readonly FFloat32 PI = new(FFloat32.DefaultScale, 12868); //PI x 2^12
        public static readonly FFloat32 TwoPIF = PI * 2; //radian equivalent of 260 degrees
        public static readonly FFloat32 PIOver180F = PI / 180; //PI / 180
        public static readonly FFloat32 OneF = 1;

        public static FFloat32 Sqrt(FFloat32 f, int numberOfIterations)
        {
            if (f.RawValue < 0) //NaN in Math.Sqrt
                throw new ArithmeticException("Input Error");
            if (f.RawValue == 0)
                return 0;
            FFloat32 k = f + OneF >> 1;
            for (int i = 0; i < numberOfIterations; i++)
                k = (k + (f / k)) >> 1;

            if (k.RawValue < 0)
                throw new ArithmeticException("Overflow");
            else
                return k;
        }

        public static FFloat32 FromParts(int PreDecimal, int PostDecimal)
        {
            var f = new FFloat32(PreDecimal);
            if (PostDecimal != 0)
                f.RawValue += (new FFloat32(PostDecimal) / 1000).RawValue;

            return f;
        }

        public static FFloat32 Sqrt(FFloat32 f)
        {
            byte numberOfIterations = 8;
            if (f.RawValue > 0x64000)
                numberOfIterations = 12;
            if (f.RawValue > 0x3e8000)
                numberOfIterations = 16;
            return Sqrt(f, numberOfIterations);
        }

        public static FFloat32 Sin(FFloat32 i)
        {
            var defaultScale = FFloat32.DefaultScale;
            FFloat32 j = 0;
            for (; i < 0; i += new FFloat32(defaultScale, 25736)) ;
            if (i > new FFloat32(defaultScale, 25736))
                i %= new FFloat32(defaultScale, 25736);
            var k = (i * new FFloat32(defaultScale, 10)) / new FFloat32(defaultScale, 714);
            if (i != 0 && i != new FFloat32(defaultScale, 6434) && i != new FFloat32(defaultScale, 12868) &&
                i != new FFloat32(defaultScale, 19302) && i != new FFloat32(defaultScale, 25736))
                j = (i * new FFloat32(defaultScale, 100)) / new FFloat32(defaultScale, 714) -
                    k * new FFloat32(defaultScale, 10);
            if (k <= new FFloat32(defaultScale, 90))
                return sin_lookup(k, j);
            if (k <= new FFloat32(defaultScale, 180))
                return sin_lookup(new FFloat32(defaultScale) - k, j);
            if (k <= new FFloat32(defaultScale, 270))
                return sin_lookup(k - new FFloat32(defaultScale), j).Inverse;
            else
                return sin_lookup(new FFloat32(defaultScale, 360) - k, j).Inverse;
        }

        private static FFloat32 sin_lookup(FFloat32 i, FFloat32 j)
        {
            var defaultScale = FFloat32.DefaultScale;
            if (j > 0 && j < new FFloat32(defaultScale, 10) && i < new FFloat32(defaultScale, 90))
                return new FFloat32(defaultScale, SIN_TABLE[i.RawValue]) +
                       ((new FFloat32(defaultScale, SIN_TABLE[i.RawValue + 1]) -
                         new FFloat32(defaultScale, SIN_TABLE[i.RawValue])) /
                        new FFloat32(defaultScale, 10)) * j;
            else
                return new FFloat32(defaultScale, SIN_TABLE[i.RawValue]);
        }

        private static int[] SIN_TABLE =
        {
            0, 71, 142, 214, 285, 357, 428, 499, 570, 641,
            711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333,
            1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985,
            2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577,
            2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091,
            3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510,
            3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823,
            3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020,
            4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095,
            4096
        };
        
        public static FFloat32 Cos( FFloat32 i )
        {
            return Sin( i + new FFloat32(FFloat32.DefaultScale, 6435));
        }

        public static FFloat32 Tan( FFloat32 i )
        {
            return Sin( i ) / Cos( i );
        }

        public static FFloat32 Abs( FFloat32 F )
        {
            if ( F < 0 )
                return F.Inverse;
            else
                return F;
        }
    }
}