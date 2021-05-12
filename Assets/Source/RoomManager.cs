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
        [Flags]
        enum ButtonsState
        {
            Random = 1 << 1,
            New = 1 << 2,
            Disconnect = 1 << 3,
            
            None = 0,
            OnMaster = Random | New,
            InRoom = Disconnect,
            All = Random | New | Disconnect
        }

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

            PhotonNetwork.GameVersion = "0.1";
            PhotonNetwork.ConnectUsingSettings();
            PhotonPeer.RegisterType(typeof(PaintSphereHitData), (byte) 'b', PaintSphereHitData.Serialize,
                PaintSphereHitData.Deserialize);

            randomButton.onClick.AddListener(RandomButtonClick);
            newButton.onClick.AddListener(NewButtonClick);
            disconnectButton.onClick.AddListener(DisconnectButtonClick);
            
            ApplyButtonState(ButtonsState.None);
        }

        private void ApplyButtonState(ButtonsState buttonsState)
        {
            newButton.interactable = buttonsState.HasFlag(ButtonsState.New);
            randomButton.interactable = buttonsState.HasFlag(ButtonsState.Random);
            disconnectButton.interactable = buttonsState.HasFlag(ButtonsState.Disconnect);
        }

        private void DisconnectButtonClick()
        {
            ApplyButtonState(ButtonsState.None);
            _paintTextureNetworking.ResetTexture();
            PhotonNetwork.LeaveRoom();
        }

        private void NewButtonClick()
        {
            ApplyButtonState(ButtonsState.None);
            _paintTextureNetworking.ResetTexture();
            PhotonNetwork.CreateRoom(Random.Range(0, 99).ToString("00"));
        }

        private void RandomButtonClick()
        {
            ApplyButtonState(ButtonsState.None);
            _paintTextureNetworking.ResetTexture();
            if (!PhotonNetwork.JoinRandomRoom())
                OnJoinRandomFailed(-1, string.Empty);
        }

        public override void OnConnectedToMaster()
        {
            errorsLabel.text = string.Empty;
            ApplyButtonState(ButtonsState.OnMaster);
        }

        private void Update()
        {
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                roomLabel.text = $"<color=#ffaa00>On Master Server</color> rooms online {PhotonNetwork.CountOfRooms}";
        }

        public override void OnJoinedRoom()
        {
            roomLabel.text = $"<color=#00aaaa>Room: {PhotonNetwork.CurrentRoom.Name}</color>";
            errorsLabel.text = string.Empty;
            ApplyButtonState(ButtonsState.InRoom);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            errorsLabel.text = $"Random failed - {message}";
            ApplyButtonState(ButtonsState.OnMaster);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            errorsLabel.text = $"Create failed - {message}";
            ApplyButtonState(ButtonsState.OnMaster);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }
}