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
    [RequireComponent(typeof(P3dPaintSphere))]
    public class PaintSphereNetworking : MonoBehaviour,
        IPaintableTextureNetworkingHitPoint,
        IOnEventCallback,
        IInRoomCallbacks
    {
        public event Action<float> OnResourceLoadStarted;
        public event Action OnResourceLoadEnded;

        private const float SendCooldown = 1f; // how often brush cache is being sent in seconds

        private const int MaxPhotonDataSize = 50; // (KB)
        // https://doc.photonengine.com/en-us/pun/current/troubleshooting/faq#can_i_send_a_huge_message_using_photon_

        private readonly List<PaintSphereHitData> _brushViewHitDataLocalCache = new List<PaintSphereHitData>(1024);

        private readonly List<PaintSphereHitData> _totalBrushViewHitDataCache =
            new List<PaintSphereHitData>(1024 * 50 / PaintSphereHitData.Size);

        private bool _sendCache = true;
        private P3dPaintSphere _remotePaintSphere;
        private P3dPaintableTexture _paintableTexture;

        private readonly RaiseEventOptions _sendCacheEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        private void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);

            _remotePaintSphere = GetComponent<P3dPaintSphere>();
            _paintableTexture = _remotePaintSphere.TargetTexture;
            if (_paintableTexture is null)
                throw new NullReferenceException($"Set Paintable Texture parameter on {_remotePaintSphere}");
        }

        private void Start()
        {
            var textureSize = GetTextureSize(out _);
            if (textureSize > MaxPhotonDataSize)
                Debug.LogWarning("Target texture exceeds recommended max size");

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
            texture = _paintableTexture.GetPngData();
            return texture.Length / 1024f;
        }

        private float GetTotalCacheSize()
        {
            return _totalBrushViewHitDataCache.Count * PaintSphereHitData.Size / 1024f;
        }

        public void ResetTexture()
        {
            _paintableTexture.Clear();
            _brushViewHitDataLocalCache.Clear();
            _totalBrushViewHitDataCache.Clear();
        }

        public void NetworkHitPoint(PaintSphereHitData paintSphereHitData)
        {
            _brushViewHitDataLocalCache.Add(paintSphereHitData);
            _totalBrushViewHitDataCache.Add(paintSphereHitData);
        }

        private void RemotePaintSphere(PaintSphereHitData paintSphereHitData)
        {
            _remotePaintSphere.Color = paintSphereHitData.Color;
            _remotePaintSphere.Radius = paintSphereHitData.BrushSize;
            if (_remotePaintSphere.BlendMode.Index != paintSphereHitData.BlendModeIndex)
                _remotePaintSphere.BlendMode = paintSphereHitData.BlendModeIndex switch
                {
                    P3dBlendMode.ALPHA_BLEND => P3dBlendMode.AlphaBlend(Vector4.one),
                    P3dBlendMode.REPLACE_ORIGINAL => P3dBlendMode.ReplaceOriginal(Vector4.one),
                    _ => throw new NotImplementedException()
                };

            _remotePaintSphere.HandleHitPoint(false, 0, 1, Random.Range(int.MinValue, int.MaxValue),
                paintSphereHitData.Position,
                Quaternion.identity);
        }
        
        private void SendCache()
        {
            if (_brushViewHitDataLocalCache.Count == 0 || !PhotonNetwork.InRoom)
                return;

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
                TargetActors = new[] {actorNumber}
            };

            var textureSize = GetTextureSize(out var texture);
            var totalCacheSize = GetTotalCacheSize();

            if (totalCacheSize < textureSize)
            {
                PhotonNetwork.RaiseEvent(
                    NetworkEvents.ResourceLoadStart,
                    totalCacheSize,
                    raiseEventOptions,
                    SendOptions.SendReliable);

                PhotonNetwork.RaiseEvent(
                    NetworkEvents.BrushTotalCache,
                    _totalBrushViewHitDataCache.ToArray(),
                    raiseEventOptions,
                    SendOptions.SendReliable);
            }
            else
            {
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
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case NetworkEvents.BrushCache:
                    var brushViewHitArray = (PaintSphereHitData[]) photonEvent.CustomData;
                    foreach (var brushViewHitData in brushViewHitArray)
                    {
                        RemotePaintSphere(brushViewHitData);
                        if (PhotonNetwork.IsMasterClient)
                            _totalBrushViewHitDataCache.Add(brushViewHitData);
                    }

                    break;

                case NetworkEvents.ResourceLoadStart:
                    var size = (float) photonEvent.CustomData;
                    OnResourceLoadStarted?.Invoke(size);
                    break;

                case NetworkEvents.Texture:
                    var texturePngRaw = (byte[]) photonEvent.CustomData;
                    _paintableTexture.LoadFromData(texturePngRaw);
                    OnResourceLoadEnded?.Invoke();
                    break;

                case NetworkEvents.BrushTotalCache:
                    var totalBrushViewHitArray = (PaintSphereHitData[]) photonEvent.CustomData;
                    foreach (var totalBrushViewHit in totalBrushViewHitArray)
                        RemotePaintSphere(totalBrushViewHit);
                    OnResourceLoadEnded?.Invoke();
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
            Debug.LogError("Master client switch is not supported");
        }
    }
}