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
        [SerializeField] private Blocker blocker;

        private const float SendCooldown = 1f; // how often brush cache is being sent in seconds

        private readonly List<BrushViewHitData> _brushViewHitDataLocalCache = new List<BrushViewHitData>(1024);
        private readonly List<BrushViewHitData> _totalBrushViewHitDataCache = new List<BrushViewHitData>(1024);
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

        public void ResetTexture()
        {
            paintableTexture.Clear();
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
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                SendOptions.SendReliable);
            _brushViewHitDataLocalCache.Clear();
        }

        private void SendTexture(int targetActor)
        {
            var eventContent = paintableTexture.GetPngData();
            var textureSize = eventContent.Length / 1024f;
            print($"sent texture {textureSize} kbytes");

            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] {targetActor},
                CachingOption = EventCaching.AddToRoomCache
            };

            PhotonNetwork.RaiseEvent(
                NetworkEvents.TextureHead,
                textureSize,
                raiseEventOptions,
                SendOptions.SendReliable);

            PhotonNetwork.RaiseEvent(
                NetworkEvents.Texture,
                eventContent,
                raiseEventOptions,
                SendOptions.SendReliable);
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
                
                case NetworkEvents.Texture:
                    print("received texture");
                    var texturePngRaw = (byte[]) photonEvent.CustomData;
                    paintableTexture.LoadFromData(texturePngRaw);
                    blocker.SetVisible(false);
                    break;

                case NetworkEvents.TextureHead:
                    var textureSize = (float) photonEvent.CustomData;
                    print($"received texture head {textureSize}");
                    blocker.Text = $"Please, wait...\n\nDownloading resources:\n{textureSize:0.00} kbytes";
                    blocker.SetVisible(true);
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