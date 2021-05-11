using ExitGames.Client.Photon;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public Color32 Color;
        public byte BrushSize;
        public byte BlendModeIndex;
        public Vector3 Position;

        public const byte Size =
            3 * sizeof(byte) + // color32
            1 * sizeof(byte) + // brushSize
            1 * sizeof(byte) + // blendModeIndex
            3 * sizeof(float); // position

        private static readonly byte[] Mem = new byte[Size];

        public static short Serialize(StreamBuffer outStream, object obj)
        {
            var brushViewHitData = (PaintSphereHitData) obj;
            lock (Mem)
            {
                var bytes = Mem;
                var index = 0;
                bytes[index++] = brushViewHitData.Color.r;
                bytes[index++] = brushViewHitData.Color.g;
                bytes[index++] = brushViewHitData.Color.b;
                bytes[index++] = brushViewHitData.BrushSize;
                bytes[index++] = brushViewHitData.BlendModeIndex;
                Protocol.Serialize(brushViewHitData.Position.x, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.y, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.z, bytes, ref index);

                outStream.Write(bytes, 0, Size);
            }

            return Size;
        }

        public static object Deserialize(StreamBuffer inStream, short size)
        {
            var brushViewHitData = new PaintSphereHitData();
            lock (Mem)
            {
                inStream.Read(Mem, 0, size);

                var index = 0;
                brushViewHitData.Color.r = Mem[index++];
                brushViewHitData.Color.g = Mem[index++];
                brushViewHitData.Color.b = Mem[index++];
                brushViewHitData.Color.a = byte.MaxValue;
                brushViewHitData.BrushSize = Mem[index++];
                brushViewHitData.BlendModeIndex = Mem[index++];
                Protocol.Deserialize(out brushViewHitData.Position.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.z, Mem, ref index);
            }

            return brushViewHitData;
        }
    }
}