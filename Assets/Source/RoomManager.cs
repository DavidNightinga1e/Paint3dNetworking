using System;
using ExitGames.Client.Photon;
using Photon.Pun;
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
        [SerializeField] private Button randomButton;
        [SerializeField] private Button newButton;
        [SerializeField] private Button disconnectButton;

        private BrushNetworking _brushNetworking;

        private void Awake()
        {
            _brushNetworking = FindObjectOfType<BrushNetworking>();
            
            roomLabel.text = "<color=#ff0000>Connecting to master</color>";
            PhotonNetwork.ConnectUsingSettings();
            PhotonPeer.RegisterType(typeof(BrushViewHitData), (byte) 'b', BrushViewHitData.Serialize,
                BrushViewHitData.Deserialize);

            randomButton.onClick.AddListener(RandomButtonClick);
            newButton.onClick.AddListener(NewButtonClick);
            disconnectButton.onClick.AddListener(DisconnectButtonClick);
        }

        private void DisconnectButtonClick()
        {
            _brushNetworking.ResetTexture();
            PhotonNetwork.LeaveRoom();
        }

        private void NewButtonClick()
        {
            _brushNetworking.ResetTexture();
            PhotonNetwork.CreateRoom(Random.Range(0, 99).ToString("00"));
        }

        private void RandomButtonClick()
        {
            _brushNetworking.ResetTexture();
            if (!PhotonNetwork.JoinRandomRoom())
                OnJoinRandomFailed(-1, string.Empty);
        }

        public override void OnConnectedToMaster()
        {
            roomLabel.text = "<color=#00aa00>On Master Server</color>";
            
        }

        public override void OnJoinedRoom()
        {
            roomLabel.text = $"<color=#00aaaa>Room: {PhotonNetwork.CurrentRoom.Name}</color>";
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            roomLabel.text = $"<color=#ff0000>Random failed - {returnCode}</color>";
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            roomLabel.text = $"<color=#ff0000>Create failed - {returnCode}</color>";
        }
    }
}