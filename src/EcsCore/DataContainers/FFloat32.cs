using System;
using System.Globalization;

namespace EcsCore
{
    public struct FFloat32 : IEquatable<FFloat32>
    {
        public const int Epsilon = 1;

        public const int DefaultScale = 16;

        private const int FractionMask = 0xffff;


        public FFloat32(int scale) : this(scale, 0)
        {
        }

        public int Scale { get; internal set; }

        public long RawValue { get; internal set; }

        public FFloat32 Inverse
        {
            get
            {
                var inverse = new FFloat32(0, 0);
                inverse.Scale = Scale;
                inverse.RawValue = -RawValue;
                return inverse;
            }
        }

        public FFloat32(int scale, int wholeNumber)
        {
            Scale = scale;
            RawValue = wholeNumber << scale;
        }

        public int WholeNumber =>
            (int)(RawValue >> Scale) +
            (RawValue < 0 && Fraction != 0 ? 1 : 0);

        public int Fraction => (int)(RawValue & FractionMask);

        public bool Equals(FFloat32 other)
        {
            return Scale == other.Scale && RawValue == other.RawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is FFloat32 other && Equals(other);
        }


        public static explicit operator float(FFloat32 number)
        {
            return (float)number.RawValue / (1 << number.Scale);
        }

        public static implicit operator FFloat32(int number)
        {
            return new FFloat32(DefaultScale, number);
        }

        public static implicit operator int(FFloat32 number)
        {
            return number.WholeNumber;
        }


        public static FFloat32 operator +(FFloat32 leftHandSide, FFloat32 rightHandSide)
        {
            leftHandSide.RawValue += rightHandSide.RawValue;

            return leftHandSide;
        }

        public static FFloat32 operator -(FFloat32 leftHandSide, FFloat32 rightHandSide)
        {
            leftHandSide.RawValue -= rightHandSide.RawValue;

            return leftHandSide;
        }

        public static FFloat32 operator *(FFloat32 leftHandSide, FFloat32 rightHandSide)
        {
            var result = leftHandSide.RawValue * rightHandSide.RawValue;

            leftHandSide.RawValue = result >> leftHandSide.Scale;

            return leftHandSide;
        }


        public static FFloat32 operator /(FFloat32 leftHandSide, FFloat32 rightHandSide)
        {
            var result = (leftHandSide.RawValue << leftHandSide.Scale) / rightHandSide.RawValue;

            leftHandSide.RawValue = result;

            return leftHandSide;
        }

        public static bool operator ==(FFloat32 one, FFloat32 other)
        {
            return one.RawValue == other.RawValue;
        }

        public static bool operator ==(FFloat32 one, int other)
        {
            return one == (FFloat32)other;
        }

        public static bool operator ==(int other, FFloat32 one)
        {
            return (FFloat32)other == one;
        }

        public static bool operator !=(FFloat32 one, FFloat32 other)
        {
            return one.RawValue != other.RawValue;
        }

        public static bool operator !=(FFloat32 one, int other)
        {
            return one != (FFloat32)other;
        }

        public static bool operator !=(int other, FFloat32 one)
        {
            return (FFloat32)other != one;
        }

        public static bool operator >=(FFloat32 one, FFloat32 other)
        {
            return one.RawValue >= other.RawValue;
        }

        public static bool operator >=(FFloat32 one, int other)
        {
            return one >= (FFloat32)other;
        }

        public static bool operator >=(int other, FFloat32 one)
        {
            return (FFloat32)other >= one;
        }

        public static bool operator <=(FFloat32 one, FFloat32 other)
        {
            return one.RawValue <= other.RawValue;
        }

        public static bool operator <=(FFloat32 one, int other)
        {
            return one <= (FFloat32)other;
        }

        public static bool operator <=(int other, FFloat32 one)
        {
            return (FFloat32)other <= one;
        }

        public static bool operator >(FFloat32 one, FFloat32 other)
        {
            return one.RawValue > other.RawValue;
        }

        public static bool operator >(FFloat32 one, int other)
        {
            return one > (FFloat32)other;
        }

        public static bool operator >(int other, FFloat32 one)
        {
            return (FFloat32)other > one;
        }

        public static bool operator <(FFloat32 one, FFloat32 other)
        {
            return one.RawValue < other.RawValue;
        }

        public static bool operator <(FFloat32 one, int other)
        {
            return one < (FFloat32)other;
        }

        public static bool operator <(int other, FFloat32 one)
        {
            return (FFloat32)other < one;
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public override string ToString()
        {
            return ((float)this).ToString(CultureInfo.InvariantCulture);
        }
    }
}