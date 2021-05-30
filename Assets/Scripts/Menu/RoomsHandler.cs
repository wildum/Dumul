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
        ReadyToStart,
        Started
    }

    public enum RoomType
    {
        Waiting,
        Lobby
    }

    public class RoomsHandler : MonoBehaviourPunCallbacks
    {
        private RoomState roomState = RoomState.Chill;
        private byte numberOfPlayerExpected = 4;
        private ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
        public const string stateProperty = "currentState";
        public const string roomTypeProperty = "roomType";
        public const string aiDifficultyProperty = "aiDifficulty";

        public void handle1v1Matchmaking()
        {
            Debug.Log("1 v 1");
            myCustomProperties.Remove(aiDifficultyProperty);
            handleMatchmaking(State.OneVsOne, 2);
        }

        public void handle2v2Matchmaking()
        {
            Debug.Log("2 v 2");
            myCustomProperties.Remove(aiDifficultyProperty);
            handleMatchmaking(State.TwoVsTwo, 4);
        }

        public void startPractice()
        {
            Debug.Log("Practice");
            myCustomProperties.Remove(aiDifficultyProperty);
            handleMatchmaking(State.Practice, 1);
        }

        public void startOneVsAI(AiDifficulty difficulty)
        {
            Debug.Log("1 v AI");
            myCustomProperties[aiDifficultyProperty] = difficulty;
            handleMatchmaking(State.OneVsAI, 1);
        }

        public void startTwoVsAI(AiDifficulty difficulty)
        {
            Debug.Log("2 v AI");
            myCustomProperties[aiDifficultyProperty] = difficulty;
            handleMatchmaking(State.TwoVsAI, 2);
        }

        public void handleMatchmaking(State state, byte nbPlayers)
        {
            myCustomProperties[stateProperty] = (int) state;
            myCustomProperties[roomTypeProperty] = (int) RoomType.Lobby;
            PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
            numberOfPlayerExpected = nbPlayers;
            if (PhotonNetwork.CurrentRoom.PlayerCount == nbPlayers)
            {
                roomState = RoomState.ReadyToStart;
            }
            else
            {
                roomState = RoomState.Searching;
                PhotonNetwork.LeaveRoom();
            }
        }

        public void loadArena()
        {
            roomState = RoomState.Started;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Arena");
        }

        public void joinRandomRoom()
        {
            Debug.Log("Join random room with property : " + myCustomProperties[stateProperty]);
            PhotonNetwork.JoinRandomRoom(myCustomProperties, numberOfPlayerExpected);
        }

        public void startGameIfFull()
        {
            if (PhotonNetwork.IsMasterClient
                && PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayerExpected
                && getMyCurrentRoomType() == RoomType.Lobby)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
                roomState = RoomState.ReadyToStart;
                Debug.Log("ready to start!");
            }
        }

        public void createLobbyRoom()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = numberOfPlayerExpected;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            myCustomProperties[roomTypeProperty] = (int) RoomType.Lobby;
            string[] roomPropsInLobby;
            if ((State) myCustomProperties[stateProperty] == State.TwoVsAI)
            {
                roomPropsInLobby = new string[] { stateProperty, roomTypeProperty, aiDifficultyProperty };
            }
            else
            {
                roomPropsInLobby = new string[] { stateProperty, roomTypeProperty };
            }
            roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
            roomOptions.CustomRoomProperties = myCustomProperties;
            Debug.Log("Create a lobby room with properties " + myCustomProperties[stateProperty]);
            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void createWaitingRoom()
        {
            Photon.Realtime.RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 4;
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = true;
            myCustomProperties[roomTypeProperty] = (int) RoomType.Waiting;
            myCustomProperties[stateProperty] = (int) State.WaitingRoom;
            string[] roomPropsInLobby =  { stateProperty, roomTypeProperty };
            roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
            roomOptions.CustomRoomProperties = myCustomProperties;
            Debug.Log("Create a waiting room with properties " + myCustomProperties[stateProperty]);
            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public State getMyCurrentRoomState()
        {
            return (State) myCustomProperties[stateProperty];
        }

        public RoomType getMyCurrentRoomType()
        {
            return (RoomType) myCustomProperties[roomTypeProperty];
        }

        public RoomState RoomState { get {return roomState;} set {roomState = value;} }
    }
}