﻿using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

namespace menu
{
    public enum RoomState
    {
        Chill,
        Searching
    }
    public class RoomsHandler : MonoBehaviourPunCallbacks
    {
        private RoomState roomState = RoomState.Chill;
        private byte numberOfPlayerExpected = 4;
        public void handle1v1Matchmaking()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                Debug.Log("load scene 1 v 1");
                AppState.currentState = State.OneVsOne;
                PhotonNetwork.LoadLevel("Arena");
            }
            else
            {
                roomState = RoomState.Searching;
                numberOfPlayerExpected = 2;
                // when matchmaking with more people, they should all come together
                PhotonNetwork.LeaveRoom();
            }
        }

        public void startPratice()
        {
            Debug.Log("Pratice");
            AppState.currentState = State.Pratice;
            PhotonNetwork.LoadLevel("Arena");
        }

        public void startOneVsAI()
        {
            Debug.Log("OneVsAI");
            AppState.currentState = State.OneVsAI;
            PhotonNetwork.LoadLevel("Arena");
        }

        public void joinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom(null, numberOfPlayerExpected);
        }

        public void startGameIfFull()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayerExpected)
            {
                Debug.Log("load scene 1 v 1");
                AppState.currentState = State.OneVsOne;
                PhotonNetwork.LoadLevel("Arena");
            }
        }

        public void resetRoom()
        {
            roomState = RoomState.Chill;
            PhotonNetwork.CurrentRoom.MaxPlayers = 4;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        public void createWaitingRoom()
        {
            Debug.Log("Create a waiting room");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = numberOfPlayerExpected;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public RoomState RoomState { get {return roomState;} }
    }
}