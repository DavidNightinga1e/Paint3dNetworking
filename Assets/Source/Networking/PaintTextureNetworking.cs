using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using PaintIn3D;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Source.Networking
{
    /// <summary>
    /// Handles networking for PaintTexture using Local and Remote brushes 
    /// </summary>
    public class PaintTextureNetworking : MonoBehaviour,
        IOnEventCallback,
        IInRoomCallbacks
    {
        #region Public Settings

        [Header("Brushes")] public RemoteBrush remoteBrush;
        public LocalBrush localBrush;

        [Header("Paintable texture")] public P3dPaintableTexture paintableTexture;

        #endregion

        #region Public Events

        public event Action<float> OnResourceLoadStarted;
        public event Action OnResourceLoadEnded;

        #endregion

        #region Private Settings

        private const float SendCooldown = 1f; // how often brush cache is being sent in seconds

        private const int MaxPhotonDataSize = 50; // (KB)
        // https://doc.photonengine.com/en-us/pun/current/troubleshooting/faq#can_i_send_a_huge_message_using_photon_

        private bool _sendCache = true; // send cache? Basically replaces while(true) loop in coroutine

        private readonly RaiseEventOptions _sendCacheEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        #endregion

        #region Cache

        private readonly List<PaintSphereHitData> _brushViewHitDataLocalCache = new List<PaintSphereHitData>(1024);

        private readonly List<PaintSphereHitData> _totalBrushViewHitDataCache =
            new List<PaintSphereHitData>(1024 * 50 / PaintSphereHitData.Size);

        private bool _recordTotalCache = true;
        private float _textureSize;

        #endregion

        private float TotalCacheSize => _totalBrushViewHitDataCache.Count * PaintSphereHitData.Size / 1024f;

        public void ResetTexture()
        {
            paintableTexture.Clear();
            _brushViewHitDataLocalCache.Clear();
            _totalBrushViewHitDataCache.Clear();
        }

        private void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);

            localBrush.OnBrushPaint += OnLocalBrushPaint;

            if (paintableTexture == null)
                throw new UnassignedReferenceException(nameof(paintableTexture));
        }

        private void OnLocalBrushPaint(PaintSphereHitData hitData)
        {
            _brushViewHitDataLocalCache.Add(hitData);
            AddTotalCache(hitData);
        }

        private void Start()
        {
            _textureSize = GetTexture(out _);
            if (_textureSize > MaxPhotonDataSize)
                Debug.LogWarning("Target texture exceeds recommended max size");

            StartCoroutine(SendCacheCoroutine());

            IEnumerator SendCacheCoroutine()
            {
                while (_sendCache)
                {
                    yield return new WaitForSecondsRealtime(SendCooldown);

                    if (_brushViewHitDataLocalCache.Count == 0 || !PhotonNetwork.InRoom)
                        continue;

                    PhotonNetwork.RaiseEvent(
                        NetworkEvents.BrushCache,
                        _brushViewHitDataLocalCache.ToArray(),
                        _sendCacheEventOptions,
                        SendOptions.SendReliable);
                    _brushViewHitDataLocalCache.Clear();
                }
            }
        }

        private float GetTexture(out byte[] texture)
        {
            texture = paintableTexture.GetPngData();
            return texture.Length / 1024f;
        }

        private void AddTotalCache(PaintSphereHitData paintSphereHitData)
        {
            if (!_recordTotalCache)
                return;

            _totalBrushViewHitDataCache.Add(paintSphereHitData);

            if (!(TotalCacheSize > _textureSize))
                return;

            _recordTotalCache = false;
            _totalBrushViewHitDataCache.Clear();
        }

        private void SendCurrentState(int actorNumber)
        {
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] {actorNumber}
            };

            void SendEvent(byte code, object payload)
            {
                PhotonNetwork.RaiseEvent(
                    code,
                    payload,
                    raiseEventOptions,
                    SendOptions.SendReliable);
            }

            if (_recordTotalCache)
            {
                SendEvent(NetworkEvents.ResourceLoadStart, TotalCacheSize);
                SendEvent(NetworkEvents.BrushTotalCache, _totalBrushViewHitDataCache.ToArray());
            }
            else
            {
                var size = GetTexture(out var texture);
                SendEvent(NetworkEvents.ResourceLoadStart, size);
                SendEvent(NetworkEvents.Texture, texture);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case NetworkEvents.BrushCache:
                    HandleCacheArray((PaintSphereHitData[]) photonEvent.CustomData);
                    break;

                case NetworkEvents.ResourceLoadStart:
                    var size = (float) photonEvent.CustomData;
                    OnResourceLoadStarted?.Invoke(size);
                    break;

                case NetworkEvents.Texture:
                    var texturePngRaw = (byte[]) photonEvent.CustomData;
                    paintableTexture.LoadFromData(texturePngRaw);
                    _recordTotalCache = false;
                    OnResourceLoadEnded?.Invoke();
                    break;

                case NetworkEvents.BrushTotalCache:
                    HandleCacheArray((PaintSphereHitData[]) photonEvent.CustomData);
                    OnResourceLoadEnded?.Invoke();
                    break;
            }

            void HandleCacheArray(PaintSphereHitData[] cacheArray)
            {
                foreach (var viewHit in cacheArray)
                {
                    remoteBrush.HandlePaintSphere(viewHit);
                    AddTotalCache(viewHit);
                }
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