﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Oculus.Platform;
using Oculus.Platform.Models;

namespace menu
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public RoomsHandler roomHandler;
        public ButtonManager buttonManager;
        // Start is called before the first frame update
        void Start()
        {
            ConnectedToServer();
            Core.Initialize();
            Entitlements.IsUserEntitledToApplication().OnComplete(callbackMethod);
            updateFriendsList();
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
            if (roomHandler.RoomState == RoomState.Searching)
            {
                roomHandler.joinRandomRoom();
            }
            else
            {
                Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions();
                roomOptions.MaxPlayers = 4;
                roomOptions.IsVisible = false;
                roomOptions.IsOpen = true;
                PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
                PhotonNetwork.AutomaticallySyncScene = true;
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined a room");
            base.OnJoinedRoom();
            if (roomHandler.RoomState != RoomState.Searching)
            {
                buttonManager.updateButtonsStatus();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            roomHandler.createWaitingRoom();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log("A new player joined the room");
            base.OnPlayerEnteredRoom(newPlayer);
            if (roomHandler.RoomState == RoomState.Chill)
            {
                buttonManager.updateButtonsStatus();
            }
            else
            {
                roomHandler.startGameIfFull();
            }
        }

        public override void OnPlayerLeftRoom(Player newPlayer)
        {
            base.OnPlayerLeftRoom(newPlayer);
            buttonManager.updateButtonsStatus();
            if (roomHandler.RoomState == RoomState.Searching)
            {
                roomHandler.resetRoom();
            }
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
                Debug.LogError("Init for the ");
            }
            else
            {
                Debug.Log("Init went fine");
            }
        }
    }
}