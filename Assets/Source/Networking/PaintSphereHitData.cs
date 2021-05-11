using ExitGames.Client.Photon;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public Color32 Color;
        public float BrushSize;
        public Vector3 Position;
        public byte BlendModeIndex;

        public const byte Size = 3 * sizeof(byte) * (1 + 3) * sizeof(float) + sizeof(byte);

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
                Protocol.Serialize(brushViewHitData.BrushSize, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.x, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.y, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.z, bytes, ref index);
                bytes[index] = brushViewHitData.BlendModeIndex;

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
                Protocol.Deserialize(out brushViewHitData.BrushSize, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.z, Mem, ref index);
                brushViewHitData.BlendModeIndex = Mem[index];
            }

            return brushViewHitData;
        }
    }
}