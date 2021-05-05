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
    public class BrushNetworking : MonoBehaviour, IHit, IHitPoint, IOnEventCallback, IInRoomCallbacks
    {
        public IBrushColorProvider brushColorProvider;
        public IBrushSizeProvider brushSizeProvider;
        public P3dPaintableTexture paintableTexture;
        public P3dPaintSphere paintSphere;

        private const float SendCooldown = 1f; // how often brush cache is being sent in seconds
        private const int MaxPhotonDataSize = 50; // (KB) https://doc.photonengine.com/en-us/pun/current/troubleshooting/faq#can_i_send_a_huge_message_using_photon_

        private readonly List<BrushViewHitData> _brushViewHitDataLocalCache = new List<BrushViewHitData>(1024);
        private readonly List<BrushViewHitData> _totalBrushViewHitDataCache = new List<BrushViewHitData>(1024 * 50 / BrushViewHitData.Size);

        private bool _sendCache = true;

        public event Action OnResourceLoadStarted;
        public event Action OnResourceLoadEnded;

        private readonly RaiseEventOptions _sendCacheEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        private void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            var textureSize = GetTextureSize(out _);
            if (textureSize > MaxPhotonDataSize)
                Debug.LogError("Target texture exceeds recommended max size");

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

        private float GetTextureSize(out byte[] texture)
        {
            texture = paintableTexture.GetPngData();
            return texture.Length / 1024f;
        }

        private float GetTotalCacheSize()
        {
            return _totalBrushViewHitDataCache.Count * BrushViewHitData.Size / 1024f;
        }

        public void ResetTexture()
        {
            paintableTexture.Clear();
        }

        public void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position,
            Quaternion rotation)
        {
            if (preview)
            {
                paintSphere.Color = brushColorProvider.Color;
                paintSphere.Radius = brushSizeProvider.BrushSize;
                paintSphere.HandleHitPoint(true, 0, 1, seed, position, rotation);
            }
            else
            {
                var brushViewHitData = new BrushViewHitData
                {
                    Color = brushColorProvider.Color,
                    Position = position,
                    Rotation = rotation,
                    BrushSize = brushSizeProvider.BrushSize
                };
                _brushViewHitDataLocalCache.Add(brushViewHitData);
                _totalBrushViewHitDataCache.Add(brushViewHitData);

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
                NetworkEvents.BrushCache,
                _brushViewHitDataLocalCache.ToArray(),
                _sendCacheEventOptions,
                SendOptions.SendReliable);
            _brushViewHitDataLocalCache.Clear();
        }

        private void SendCurrentState(int actorNumber)
        {
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] {actorNumber},
                CachingOption = EventCaching.AddToRoomCache
            };

            var textureSize = GetTextureSize(out var texture);
            var totalCacheSize = GetTotalCacheSize();

            if (totalCacheSize < textureSize)
                SendTotalCache();
            else
                SendTexture();

            void SendTexture()
            {
                print($"sent texture {textureSize} kbytes");

                PhotonNetwork.RaiseEvent(
                    NetworkEvents.ResourceLoadStart,
                    textureSize,
                    raiseEventOptions,
                    SendOptions.SendReliable);

                PhotonNetwork.RaiseEvent(
                    NetworkEvents.Texture,
                    texture,
                    raiseEventOptions,
                    SendOptions.SendReliable);
            }

            void SendTotalCache()
            {
                print($"sent total cache {totalCacheSize} kbytes");

                PhotonNetwork.RaiseEvent(
                    NetworkEvents.ResourceLoadStart,
                    totalCacheSize,
                    raiseEventOptions,
                    SendOptions.SendReliable);

                PhotonNetwork.RaiseEvent(
                    NetworkEvents.BrushTotalCache,
                    _totalBrushViewHitDataCache,
                    raiseEventOptions,
                    SendOptions.SendReliable);
            }
        }


        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case NetworkEvents.BrushCache:
                    print("received cache");
                    var brushViewHitArray = (BrushViewHitData[]) photonEvent.CustomData;
                    foreach (var brushViewHitData in brushViewHitArray)
                    {
                        HandleBrushHitPoint(brushViewHitData);
                        if (PhotonNetwork.IsMasterClient)
                            _totalBrushViewHitDataCache.Add(brushViewHitData);
                    }

                    break;

                case NetworkEvents.ResourceLoadStart:
                    var size = (float) photonEvent.CustomData;
                    print($"received resource load start {size}");
                    break;

                case NetworkEvents.Texture:
                    print("received texture");
                    var texturePngRaw = (byte[]) photonEvent.CustomData;
                    paintableTexture.LoadFromData(texturePngRaw);
                    OnResourceLoadEnded?.Invoke();
                    break;

                case NetworkEvents.BrushTotalCache:
                    print("received brush total cache");
                    var totalBrushViewHitArray = (BrushViewHitData[]) photonEvent.CustomData;
                    foreach (var totalBrushViewHit in totalBrushViewHitArray)
                        HandleBrushHitPoint(totalBrushViewHit);
                    OnResourceLoadStarted?.Invoke();
                    break;
            }
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            SendCurrentState(newPlayer.ActorNumber);
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