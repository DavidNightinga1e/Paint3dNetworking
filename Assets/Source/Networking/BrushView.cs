using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using PaintIn3D;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Source.Networking
{
    public class BrushView : MonoBehaviour, IHit, IHitPoint, IOnEventCallback, IInRoomCallbacks
    {
        [SerializeField] private ColorPalette colorPalette;
        [SerializeField] private P3dPaintSphere paintSphere;
        [SerializeField] private P3dPaintableTexture paintableTexture;

        private const float SendCooldown = 1f; // how often brush cache is being sent in seconds

        private readonly List<BrushViewHitData> _brushViewHitDataLocalCache = new List<BrushViewHitData>(1024);
        private bool _sendCache = true;

        private void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            StartCoroutine(SendCacheCoroutine());
        }

        private IEnumerator SendCacheCoroutine()
        {
            while (_sendCache)
            {
                yield return new WaitForSecondsRealtime(SendCooldown);
                SendCache();
            }
        }

        public void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position,
            Quaternion rotation)
        {
            if (preview)
            {
                paintSphere.Color = colorPalette.Color;
                paintSphere.Radius = colorPalette.BrushSize;
                paintSphere.HandleHitPoint(true, 0, 1, seed, position, rotation);
            }
            else
            {
                var brushViewHitData = new BrushViewHitData
                {
                    Color = colorPalette.Color,
                    Position = position,
                    Rotation = rotation,
                    BrushSize = colorPalette.BrushSize
                };
                _brushViewHitDataLocalCache.Add(brushViewHitData);

                HandleBrushHitPoint(brushViewHitData);
            }
        }

        private void HandleBrushHitPoint(BrushViewHitData brushViewHitData)
        {
            paintSphere.Color = brushViewHitData.Color;
            paintSphere.Radius = brushViewHitData.BrushSize;
            paintSphere.HandleHitPoint(false, 0, 1, Random.Range(int.MinValue, int.MaxValue), brushViewHitData.Position,
                brushViewHitData.Rotation);
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
                SendCache();
        }

        private void SendCache()
        {
            if (_brushViewHitDataLocalCache.Count == 0)
                return;

            print("sent cache");
            PhotonNetwork.RaiseEvent(
                1,
                _brushViewHitDataLocalCache.ToArray(),
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                SendOptions.SendReliable);
            _brushViewHitDataLocalCache.Clear();
        }

        private void SendTexture(int targetActor)
        {
            print("sent texture");

            // PhotonNetwork.RaiseEvent(
            //     2,
            //     paintableTexture.GetPngData(),
            //     new RaiseEventOptions {TargetActors = new[] {targetActor}, CachingOption = EventCaching.AddToRoomCache},
            //     SendOptions.SendUnreliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 1:
                    print("got cache");
                    var brushViewHitArray = (BrushViewHitData[]) photonEvent.CustomData;
                    foreach (var brushViewHitData in brushViewHitArray)
                        HandleBrushHitPoint(brushViewHitData);
                    break;
                case 2:
                    print("got texture");
                    var texturePngRaw = (byte[]) photonEvent.CustomData;
                    paintableTexture.LoadFromData(texturePngRaw);
                    break;
            }
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
                SendTexture(newPlayer.ActorNumber);
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }
}