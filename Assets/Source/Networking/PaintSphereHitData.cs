using ExitGames.Client.Photon;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public Color Color;
        public float BrushSize;
        public Vector3 Position;
        public int BlendModeIndex;

        public const byte Size = (3 + 1 + 3) * sizeof(float) + sizeof(int);

        private static readonly byte[] Mem = new byte[Size];

        public static short Serialize(StreamBuffer outStream, object obj)
        {
            var brushViewHitData = (PaintSphereHitData) obj;
            lock (Mem)
            {
                var bytes = Mem;
                var index = 0;
                Protocol.Serialize(brushViewHitData.Color.r, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Color.g, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Color.b, bytes, ref index);
                Protocol.Serialize(brushViewHitData.BrushSize, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.x, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.y, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.z, bytes, ref index);
                Protocol.Serialize(brushViewHitData.BlendModeIndex, bytes, ref index);

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
                Protocol.Deserialize(out brushViewHitData.Color.r, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Color.g, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Color.b, Mem, ref index);
                brushViewHitData.Color.a = 1;
                Protocol.Deserialize(out brushViewHitData.BrushSize, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.z, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.BlendModeIndex, Mem, ref index);
            }

            return brushViewHitData;
        }
    }
}