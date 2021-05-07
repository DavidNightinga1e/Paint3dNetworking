using UnityEngine;

namespace Source.Networking
{
    public struct ShortColor3
    {
        public short R;
        public short G;
        public short B;

        private ShortColor3(short r, short g, short b)
        {
            R = r;
            G = g;
            B = b;
        }

        public ShortColor3(Color color) : this(
            (short) (color.r * short.MaxValue), 
            (short) (color.r * short.MaxValue),
            (short) (color.r * short.MaxValue))
        {
        }

        public Color ToColor()
        {
            return new Color((float) R / short.MaxValue, (float) G / short.MaxValue, (float) B / short.MaxValue, 1);
        }
    }
}