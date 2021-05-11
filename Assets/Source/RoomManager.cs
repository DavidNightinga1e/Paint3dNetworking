using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Source.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Source
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TextMeshProUGUI roomLabel;
        [SerializeField] private TextMeshProUGUI errorsLabel;
        [SerializeField] private Button randomButton;
        [SerializeField] private Button newButton;
        [SerializeField] private Button disconnectButton;

        private PaintTextureNetworking _paintTextureNetworking;

        private void Awake()
        {
            _paintTextureNetworking = FindObjectOfType<PaintTextureNetworking>();
            
            roomLabel.text = "<color=#ff0000>Connecting to master</color>";
            errorsLabel.text = string.Empty;
            
            PhotonNetwork.ConnectUsingSettings();
            PhotonPeer.RegisterType(typeof(PaintSphereHitData), (byte) 'b', PaintSphereHitData.Serialize,
                PaintSphereHitData.Deserialize);

            randomButton.onClick.AddListener(RandomButtonClick);
            newButton.onClick.AddListener(NewButtonClick);
            disconnectButton.onClick.AddListener(DisconnectButtonClick);
        }

        private void DisconnectButtonClick()
        {
            _paintTextureNetworking.ResetTexture();
            PhotonNetwork.LeaveRoom();
        }

        private void NewButtonClick()
        {
            _paintTextureNetworking.ResetTexture();
            PhotonNetwork.CreateRoom(Random.Range(0, 99).ToString("00"));
        }

        private void RandomButtonClick()
        {
            _paintTextureNetworking.ResetTexture();
            if (!PhotonNetwork.JoinRandomRoom())
                OnJoinRandomFailed(-1, string.Empty);
        }

        public override void OnConnectedToMaster()
        {
            roomLabel.text = "<color=#ffaa00>On Master Server</color>";
            errorsLabel.text = string.Empty;
        }

        public override void OnJoinedRoom()
        {
            roomLabel.text = $"<color=#00aaaa>Room: {PhotonNetwork.CurrentRoom.Name}</color>";
            errorsLabel.text = string.Empty;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            errorsLabel.text = $"Random failed - {returnCode}";
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            errorsLabel.text = $"Create failed - {returnCode}";
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }
}