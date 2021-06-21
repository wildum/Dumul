using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Oculus.Platform;
using Oculus.Platform.Models;
using ExitGames.Client.Photon;
using UnityEngine.UI;

namespace menu
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public RoomsHandler roomHandler;
        public ButtonManager buttonManager;
        public Text roomName;
        private GameObject spawnedPlayerPrefab;
        public const byte StartGameTextEventCode = 1;
        public const string masterTeamMateProp = "masterTeamMate";

        private const float timeDelayStartGame = 5;

        private const float TIME_RETRY_FRIENDS = 2.0f;
        private float timeRetryFriends = TIME_RETRY_FRIENDS;
        private int triesJoinFriend = 0;

        private bool lookingForFriend = false;


        // Start is called before the first frame update
        void Start()
        {
            State currentState = State.Starter;
            if (PhotonNetwork.CurrentRoom != null)
            {
                roomName.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomsHandler.stateProperty))
                    currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties[RoomsHandler.stateProperty];
            }

            updateRelatedToState(currentState);
        }

        void Update()
        {
            if (buttonManager.WaitingForFriend)
            {
                lookingForFriend = true;
                if (timeRetryFriends >= TIME_RETRY_FRIENDS)
                {
                    Debug.Log("try to join, try : " + triesJoinFriend);
                    timeRetryFriends = 0.0f;
                    triesJoinFriend++;
                    PhotonNetwork.FindFriends(new string[1]{AppState.MasterFriendId});
                }
                else
                {
                    timeRetryFriends += Time.deltaTime;
                }
            }
            else if (lookingForFriend)
            {
                lookingForFriend = false;
                roomHandler.createWaitingRoom();
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == RoomsHandler.WaitForGameTextEventCode)
            {
                buttonManager.WaitingForFriend = true;
                PhotonNetwork.LeaveRoom();
            }
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendsInfo)
        {
            if (friendsInfo.Count != 0 && friendsInfo[0].IsInRoom)
            {
                PhotonNetwork.JoinRoom(friendsInfo[0].Room);
            }
        }
        
        void updateRelatedToState(State currentState)
        {
            if (currentState == State.Starter)
            {
                ConnectedToServer();
                Core.Initialize();
            }
            else if (currentState == State.WaitingRoom)
            {
                buttonManager.updateButtonsWaitingRoom();
            }
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            if (propertiesThatChanged.ContainsKey(RoomsHandler.stateProperty))
            {
                updateRelatedToState((State) propertiesThatChanged[RoomsHandler.stateProperty]);
            }

            if (roomHandler.RoomState == RoomState.ReadyToStart)
            {
                roomHandler.setTeams();
                // if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                // {
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(StartGameTextEventCode, null, raiseEventOptions, SendOptions.SendReliable);
                    buttonManager.buttonConfigAllDeactivate();
                    Invoke("loadArena", timeDelayStartGame);
                // }
                // else
                // {
                //     loadArena();
                // }
            }
        }

        void loadArena()
        {
            roomHandler.loadArena();
        }

        void spawnWithDelay()
        {
            spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player Menu", transform.position, transform.rotation);
        }

        void ConnectedToServer()
        {
            PhotonNetwork.GameVersion = "0.0.1";
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Trying to connect to the server...");
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            if (ButtonManager.tryingToJoinFriend)
            {
                Debug.Log("TryingToJoinFriend");
                PhotonNetwork.JoinRoom(ButtonManager.roomNameToJoin);
            }
            else if (AppState.MasterFriendId != "")
            {
                Debug.Log("EnableJoinFriend");
                buttonManager.enableJoinFriend();
            }
            else if (roomHandler.RoomState == RoomState.Searching)
            {
                Debug.Log("JoinRandomRoom");
                roomHandler.joinRandomRoom();
            }
            else
            {
                Debug.Log("CreateWaitingRoom");
                roomHandler.createWaitingRoom();
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined a room");
            base.OnJoinedRoom();
            roomName.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
            roomHandler.synchRoomProperties();
            updateButtonsWithRoomType();
            if (spawnedPlayerPrefab == null)
            {
                spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player Menu", transform.position, transform.rotation);
            }

            ExitGames.Client.Photon.Hashtable customPropertiesPlayer = new ExitGames.Client.Photon.Hashtable();
            if (ButtonManager.tryingToJoinFriend || lookingForFriend)
            {
                ButtonManager.tryingToJoinFriend = false;
                buttonManager.WaitingForFriend = false;
                lookingForFriend = false;
                if (AppState.MasterFriendId == "")
                {
                    int masterClientId = PhotonNetwork.CurrentRoom.MasterClientId;
                    AppState.MasterFriendId = PhotonNetwork.CurrentRoom.Players[masterClientId].UserId;
                    customPropertiesPlayer[masterTeamMateProp] = AppState.MasterFriendId;
                }
            }
            else
            {
                AppState.MasterFriendId = "";
            }
            PhotonNetwork.LocalPlayer.CustomProperties = customPropertiesPlayer;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            Debug.Log("did not find any room, create one to wait, reason : " + message);
            if (ButtonManager.tryingToJoinFriend)
            {
                ButtonManager.tryingToJoinFriend = false;
                roomHandler.createWaitingRoom();
            }
            else
            {
                buttonManager.buttonInLobbyConfig(roomHandler.getMyCurrentRoomState());
                roomHandler.createLobbyRoom();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log("A new player joined the room");
            base.OnPlayerEnteredRoom(newPlayer);
            updateButtonsWithRoomType();
            roomHandler.startGameIfFull();
        }

        public override void OnPlayerLeftRoom(Player oldPlayer)
        {
            base.OnPlayerLeftRoom(oldPlayer);
            updateButtonsWithRoomType();
        }

        private void updateButtonsWithRoomType()
        {
            if (roomHandler.getMyCurrentRoomType() == RoomType.Waiting)
                buttonManager.updateButtonsWaitingRoom();
            if (roomHandler.getMyCurrentRoomType() == RoomType.Lobby)
                buttonManager.buttonInLobbyConfig(roomHandler.getMyCurrentRoomState());
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            if (spawnedPlayerPrefab != null)
                PhotonNetwork.Destroy(spawnedPlayerPrefab);
        }
    }
}
