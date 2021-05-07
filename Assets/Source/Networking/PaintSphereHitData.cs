using ExitGames.Client.Photon;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public ShortColor3 Color;
        public float BrushSize;
        public Vector3 Position;

        public const byte Size = 3 * sizeof(short) + (1 + 3) * sizeof(float);
        
        private static readonly byte[] Mem = new byte[Size];

        public static short Serialize(StreamBuffer outStream, object obj)
        {
            var brushViewHitData = (PaintSphereHitData) obj;
            lock (Mem)
            {
                var bytes = Mem;
                var index = 0;
                Protocol.Serialize(brushViewHitData.Color.R, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Color.G, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Color.B, bytes, ref index);
                Protocol.Serialize(brushViewHitData.BrushSize, bytes, ref index);
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
                Protocol.Deserialize(out brushViewHitData.Color.R, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Color.G, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Color.B, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.BrushSize, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.z, Mem, ref index);
            }

            return brushViewHitData;
        }
    }
}