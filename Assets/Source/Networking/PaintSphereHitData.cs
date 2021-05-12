using ExitGames.Client.Photon;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Source.Networking
{
    public struct PaintSphereHitData
    {
        public Color32 Color;
        public byte BrushSize;
        public byte BlendModeIndex;
        public half3 Position;

        public static readonly short Size = (short) UnsafeUtility.SizeOf<PaintSphereHitData>();
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
                Protocol.Serialize((short) brushViewHitData.Position.x.value, bytes, ref index);
                Protocol.Serialize((short) brushViewHitData.Position.y.value, bytes, ref index);
                Protocol.Serialize((short) brushViewHitData.Position.z.value, bytes, ref index);

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
                Protocol.Deserialize(out short x, Mem, ref index);
                Protocol.Deserialize(out short y, Mem, ref index);
                Protocol.Deserialize(out short z, Mem, ref index);
                brushViewHitData.Position = new half3
                {
                    x =
                    {
                        value = (ushort) x
                    },
                    y =
                    {
                        value = (ushort) y
                    },
                    z =
                    {
                        value = (ushort) z
                    }
                };
            }

            return brushViewHitData;
        }
    }
}