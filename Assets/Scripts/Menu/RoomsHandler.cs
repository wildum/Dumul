using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

namespace menu
{
    public enum RoomState
    {
        Chill,
        Searching,
        ReadyToStart
    }
    public class RoomsHandler : MonoBehaviourPunCallbacks
    {
        private RoomState roomState = RoomState.Chill;
        private byte numberOfPlayerExpected = 4;
        private ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
        public const string stateProperty = "currentState";

        public void handle1v1Matchmaking()
        {
            myCustomProperties[stateProperty] = (int) State.OneVsOne;
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                Debug.Log("load scene 1 v 1");
                PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
                roomState = RoomState.ReadyToStart;
            }
            else
            {
                roomState = RoomState.Searching;
                numberOfPlayerExpected = 2;
                // when matchmaking with more people, they should all come together
                PhotonNetwork.LeaveRoom();
            }
        }

        public void handle2v2Matchmaking()
        {

        }

        public void loadArena()
        {
            if (RoomState == RoomState.ReadyToStart)
            {
                PhotonNetwork.LoadLevel("Arena");
            }
            else
            {
                Debug.Log("Property was changed but the room is not ready to start");
            }
        }

        public void startPratice()
        {
            Debug.Log("Pratice");
            myCustomProperties[stateProperty] = (int)State.Pratice;
            PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
            roomState = RoomState.ReadyToStart;
        }

        public void startOneVsAI()
        {
            Debug.Log("OneVsAI");
            myCustomProperties[stateProperty] = (int)State.OneVsAI;
            PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
            roomState = RoomState.ReadyToStart;
        }

        public void startTwoVsAI()
        {
            myCustomProperties[stateProperty] = (int) State.TwoVsAI;
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                Debug.Log("load scene 2 vs AI");
                PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
                roomState = RoomState.ReadyToStart;
            }
            else
            {
                roomState = RoomState.Searching;
                numberOfPlayerExpected = 2;
                // when matchmaking with more people, they should all come together
                PhotonNetwork.LeaveRoom();
            }
        }

        public void joinRandomRoom()
        {
            Debug.Log("Join random room with property : " + myCustomProperties[stateProperty]);
            PhotonNetwork.JoinRandomRoom(myCustomProperties, numberOfPlayerExpected);
        }

        public void startGameIfFull()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayerExpected)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
                roomState = RoomState.ReadyToStart;
                Debug.Log("ready to start!");
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
            Debug.Log("Create a waiting room with properties " + myCustomProperties[stateProperty]);
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = numberOfPlayerExpected;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            string[] roomPropsInLobby =  { stateProperty };
            roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
            roomOptions.CustomRoomProperties = myCustomProperties;
            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public RoomState RoomState { get {return roomState;} }
    }
}