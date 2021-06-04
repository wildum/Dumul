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
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public RoomsHandler roomHandler;
        public ButtonManager buttonManager;
        public Text roomName;
        private GameObject spawnedPlayerPrefab;
        public const byte StartGameTextEventCode = 1;

        private const float timeDelayStartGame = 5;
        private bool findFriends = false;

        private const float TIME_RETRY_FRIENDS = 2.0f;
        private float timeRetryFriends = TIME_RETRY_FRIENDS;
        private int TRIES_JOIN_FRIEND = 5;
        private int triesJoinFriend = 0;


        // Start is called before the first frame update
        void Start()
        {
            State currentState = State.Starter;
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomsHandler.stateProperty))
            {
                currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties[RoomsHandler.stateProperty];
            }

            updateRelatedToState(currentState);
        }

        void Update()
        {
            if (findFriends)
            {
                if (triesJoinFriend > TRIES_JOIN_FRIEND)
                {
                    Debug.Log("did not found friend");
                    resetFriendsParam();
                }
                else if (timeRetryFriends >= TIME_RETRY_FRIENDS)
                {
                    timeRetryFriends = 0.0f;
                    triesJoinFriend++;
                    PhotonNetwork.FindFriends(new string[1]{AppState.MasterFriendId});
                }
                else
                {
                    timeRetryFriends += Time.deltaTime;
                }
            }
        }

        void resetFriendsParam()
        {
            findFriends = false;
            triesJoinFriend = 0;
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendsInfo)
        {
            if (friendsInfo.Count != 0 && friendsInfo[0].IsInRoom)
            {
                resetFriendsParam();
                PhotonNetwork.JoinRoom(friendsInfo[0].Room);
            }
        }
        
        void updateRelatedToState(State currentState)
        {
            if (currentState == State.Starter)
            {
                ConnectedToServer();
                Core.Initialize();
                Entitlements.IsUserEntitledToApplication().OnComplete(callbackMethod);
                updateFriendsList();
            }
            else if (currentState == State.WaitingRoom)
            {
                buttonManager.updateButtonsWaitingRoom();
                // TODO : fix this ?
                //Invoke("spawnWithDelay", 2.0f);
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
                if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(StartGameTextEventCode, null, raiseEventOptions, SendOptions.SendReliable);
                    Invoke("loadArena", timeDelayStartGame);
                }
                else
                {
                    loadArena();
                }
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
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Trying to connect to the server...");
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connect to the server");
            base.OnConnectedToMaster();
            if (ButtonManager.tryingToJoinFriend)
            {
                PhotonNetwork.JoinRoom(ButtonManager.roomNameToJoin);
            }
            else if (AppState.MasterFriendId != "")
            {
                findFriends = true;
            }
            else if (roomHandler.RoomState == RoomState.Searching)
            {
                roomHandler.joinRandomRoom();
            }
            else
            {
                roomHandler.createWaitingRoom();
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined a room");
            base.OnJoinedRoom();
            roomName.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
            updateButtonsWithRoomType();
            if (spawnedPlayerPrefab == null)
            {
                spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player Menu", transform.position, transform.rotation);
            }

            if (ButtonManager.tryingToJoinFriend)
            {
                ButtonManager.tryingToJoinFriend = false;
                int masterClientId = PhotonNetwork.CurrentRoom.MasterClientId;
                AppState.MasterFriendId = PhotonNetwork.CurrentRoom.Players[masterClientId].UserId;
            }
            else
            {
                AppState.MasterFriendId = "";
            }
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

        private void updateFriendsList()
        {
            Users.GetLoggedInUser().OnComplete((Message<User> msg) => {
                Debug.Log("User ID: " + msg.Data.ID);
                Debug.Log("Oculus ID: " + msg.Data.OculusID);
            });
            Debug.Log("GET FRIENDS LIST !");
            Users.GetLoggedInUserFriends().OnComplete((Message<UserList> msg) => {
                if (msg.IsError)
                {
                    Debug.Log(msg.GetError());
                    return;
                }

                Debug.Log("Here is the list of all your occulus friends : ");
                foreach (var friend in msg.Data)
                {
                    Debug.Log("Your friend's name is " + friend.OculusID);
                }
            });
        }

        void callbackMethod(Message msg)
        {
            if (!msg.IsError)
            {
                Debug.Log("Init? ");
            }
            else
            {
                Debug.Log("Init went fine");
            }
        }
    }
}
