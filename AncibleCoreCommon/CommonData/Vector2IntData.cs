using System;

namespace AncibleCoreCommon.CommonData
{
    [Serializable]
    public struct Vector2IntData
    {

        public int X;
        public int Y;

        public Vector2IntData(int x, int y)
        {
            X = x;
            Y = y;
        }


        public static Vector2IntData operator +(Vector2IntData a) => a;

        public static Vector2IntData operator +(Vector2IntData a, Vector2IntData b)
        {
            return new Vector2IntData(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2IntData operator -(Vector2IntData a) => new Vector2IntData(-a.X, -a.Y);

        public static Vector2IntData operator -(Vector2IntData a, Vector2IntData b)
        {
            return new Vector2IntData(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Vector2IntData a, Vector2IntData b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2IntData a, Vector2IntData b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public bool Equals(Vector2IntData other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector2IntData data && Equals(data);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"X:{X},Y:{Y}";
        }

        public static Vector2IntData zero => new Vector2IntData(0,0);

    }
}