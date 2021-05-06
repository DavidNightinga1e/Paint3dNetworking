using ExitGames.Client.Photon;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public Color Color;
        public float BrushSize;
        public Vector3 Position;
        public Quaternion Rotation;

        public const byte Size = (4 + 1 + 3 + 4) * sizeof(float);
        
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
                Protocol.Serialize(brushViewHitData.Color.a, bytes, ref index);
                Protocol.Serialize(brushViewHitData.BrushSize, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.x, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.y, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Position.z, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Rotation.x, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Rotation.y, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Rotation.z, bytes, ref index);
                Protocol.Serialize(brushViewHitData.Rotation.w, bytes, ref index);
                
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
                Protocol.Deserialize(out brushViewHitData.Color.a, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.BrushSize, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Position.z, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Rotation.x, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Rotation.y, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Rotation.z, Mem, ref index);
                Protocol.Deserialize(out brushViewHitData.Rotation.w, Mem, ref index);
            }

            return brushViewHitData;
        }
    }
}