using System;
using PaintIn3D;
using Source.Networking;
using Unity.Mathematics;
using UnityEngine;

namespace Source
{
    /// <summary>
    /// Handles HitScreen and provides results to networking and paints it using paintSphere
    /// </summary>
    public class LocalBrush : MonoBehaviour, IHit, IHitPoint
    {
        [SerializeField] private P3dPaintSphere paintSphere;

        public event Action<PaintSphereHitData> OnBrushPaint; 

        public void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position,
            Quaternion rotation)
        {
            if (Input.touchCount > 1)
                return;

            if (!preview)
                OnBrushPaint?.Invoke(new PaintSphereHitData
                {
                    BrushSize = FloatUtility.ToByte(paintSphere.Radius, 0.01f, 0.51f),
                    Color = paintSphere.Color,
                    Position = new half3(new half(position.x), new half(position.y), new half(position.z)),
                    BlendModeIndex = (byte) paintSphere.BlendMode.Index
                });

            paintSphere.HandleHitPoint(preview, priority, pressure, seed, position, Quaternion.identity);
        }
    }
}