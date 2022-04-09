using System;

namespace AncibleCoreCommon.CommonData
{
    [Serializable]
    public struct Vector2Data
    {
        public float X;
        public float Y;

        public Vector2Data(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}