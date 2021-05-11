using System;

namespace Source.Networking
{
    public static class FloatUtility
    {
        /// <summary>
        /// Transforms value to byte using range. Keep in mind that range affects accuracy
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="minValue">Min value of range</param>
        /// <param name="maxValue">Max value of range</param>
        /// <returns>Byte (0-255) lossy value</returns>
        public static byte ToByte(float value, float minValue, float maxValue)
        {
            if (value > maxValue || value < minValue)
                throw new ArgumentException($"{nameof(value)} was not in the range");

            var moved = value - minValue;
            var normalized = moved / maxValue;
            var byteFloat = normalized * byte.MaxValue;
            return (byte) byteFloat;
        }

        /// <summary>
        /// Restores float from byte using range
        /// </summary>
        /// <param name="value">Packed float</param>
        /// <param name="minValue">Min value of range</param>
        /// <param name="maxValue">Max value of range</param>
        /// <returns>Restored lossy float</returns>
        public static float ToFloat(byte value, float minValue, float maxValue)
        {
            var normalized = (float) value / byte.MaxValue;
            var moved = normalized * maxValue;
            var restored = moved + minValue;
            return restored;
        }
    }
}