﻿using JetBrains.Annotations;
using PaintIn3D;
using Source.Networking;
using UnityEngine;

namespace Source
{
    public class Brush : MonoBehaviour, IHit, IHitPoint
    {
        [SerializeField] private PaintSphereNetworking paintSphereNetworking;
        [SerializeField] private P3dPaintSphere paintSphere;

        public void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position,
            Quaternion rotation)
        {
            if (Input.touchSupported && Input.touchCount > 1)
                return;

            if (!preview)
                paintSphereNetworking.NetworkHitPoint(new PaintSphereHitData
                {
                    BrushSize = paintSphere.Radius,
                    Color = paintSphere.Color,
                    Position = position,
                    Rotation = rotation
                });

            paintSphere.HandleHitPoint(preview, priority, pressure, seed, position, rotation);
        }
    }
}