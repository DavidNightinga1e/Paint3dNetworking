using System;
using PaintIn3D;
using Source.Networking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Source
{
    /// <summary>
    /// Handles remote painting and applies it on paintSphere
    /// </summary>
    public class RemoteBrush : MonoBehaviour
    {
        [SerializeField] private P3dPaintSphere paintSphere;

        public void HandlePaintSphere(PaintSphereHitData paintSphereHitData)
        {
            paintSphere.Color = paintSphereHitData.Color;
            paintSphere.Radius = FloatUtility.ToFloat(paintSphereHitData.BrushSize, 0.01f, 0.51f);
            if (paintSphere.BlendMode.Index != paintSphereHitData.BlendModeIndex)
                paintSphere.BlendMode = paintSphereHitData.BlendModeIndex switch
                {
                    P3dBlendMode.ALPHA_BLEND => P3dBlendMode.AlphaBlend(Vector4.one),
                    P3dBlendMode.REPLACE_ORIGINAL => P3dBlendMode.ReplaceOriginal(Vector4.one),
                    _ => throw new NotImplementedException()
                };
            paintSphere.HandleHitPoint(false, 0, 1, Random.Range(int.MinValue, int.MaxValue),
                new Vector3(paintSphereHitData.Position.x, paintSphereHitData.Position.y,
                    paintSphereHitData.Position.z), Quaternion.identity);
        }
    }
}