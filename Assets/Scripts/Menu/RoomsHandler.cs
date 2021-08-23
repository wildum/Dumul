using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
using System.Linq;
using ExitGames.Client.Photon;

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
        private System.Random rnd = new System.Random();
        private RoomState roomState = RoomState.Chill;
        private byte numberOfPlayerExpected = 4;
        private byte numberOfPlayersInTeam = 0;
        private ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();
        private List<string> userIds = new List<string>();
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
            userIds = new List<string>();
            if (PhotonNetwork.CurrentRoom.PlayerCount == nbPlayers)
            {
                roomState = RoomState.ReadyToStart;
            }
            else
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount != 1)
                {
                    numberOfPlayersInTeam = PhotonNetwork.CurrentRoom.PlayerCount;
                    userIds = PhotonNetwork.CurrentRoom.Players.Values.Select(p => p.UserId).ToList();
                    tellFriendsToWaitForARoom();
                }
                else
                {
                    numberOfPlayersInTeam = 0;
                }
                roomState = RoomState.Searching;
                PhotonNetwork.LeaveRoom();
            }
        }


        private void tellFriendsToWaitForARoom()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(Events.WaitForGameTextEventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }

        public bool loadArena()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayerExpected)
            {
                roomState = RoomState.Started;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel("Arena");
                return true;
            }
            roomState = RoomState.Chill;
            myCustomProperties[roomTypeProperty] = RoomType.Waiting;
            myCustomProperties[stateProperty] = State.WaitingRoom;
            PhotonNetwork.CurrentRoom.SetCustomProperties(myCustomProperties);
            return false;
        }

        public void joinRandomRoom()
        {
            Debug.Log("Join random room with property : " + myCustomProperties[stateProperty] + " and " + myCustomProperties[roomTypeProperty]);
            if (numberOfPlayersInTeam == 0)
                PhotonNetwork.JoinRandomRoom(myCustomProperties, numberOfPlayerExpected);
            else
                PhotonNetwork.JoinRandomRoom(myCustomProperties, numberOfPlayerExpected, MatchmakingMode.FillRoom, null, null, userIds.ToArray());
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

        private string getRandomRoomName()
        {
            string collection = "abcdefghijklmnopqrstuvwxyz0123456789";
            var name = new char[5];
            for (int i = 0; i < name.Length; i++)
                name[i] = collection[rnd.Next(collection.Length)];
            return new string(name); 
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
            roomOptions.PublishUserId = true;
            Debug.Log("Create a lobby room with properties " + myCustomProperties[stateProperty] + " and " + myCustomProperties[roomTypeProperty]);
            PhotonNetwork.CreateRoom(getRandomRoomName(), roomOptions, TypedLobby.Default);
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
            roomOptions.PublishUserId = true;
            Debug.Log("Create a waiting room with properties " + myCustomProperties[stateProperty]);
            bool result = PhotonNetwork.CreateRoom(getRandomRoomName(), roomOptions, TypedLobby.Default);
            if(!result)
                Debug.LogError("creating room fail?");
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void setTeams()
        {
            switch ((State) myCustomProperties[stateProperty])
            {
                case State.OneVsAI:
                case State.TwoVsAI:
                case State.Practice:
                    Debug.Log("set the team for the practice");
                    foreach (KeyValuePair<int, Photon.Realtime.Player> entry in PhotonNetwork.CurrentRoom.Players)
                    {
                        if (!entry.Value.JoinTeam(1))
                        {
                            PhotonTeam t = entry.Value.GetPhotonTeam();
                            if (t == null)
                                Debug.LogError("Join team failed");
                            else if (t.Code != 1)
                                entry.Value.SwitchTeam(1);
                        }
                    }
                    break;
                case State.OneVsOne:
                    byte team = 1;
                    foreach (KeyValuePair<int, Photon.Realtime.Player> entry in PhotonNetwork.CurrentRoom.Players)
                    {
                        if (!entry.Value.JoinTeam(team))
                        {
                            PhotonTeam t = entry.Value.GetPhotonTeam();
                            if (t == null)
                                Debug.LogError("Join team failed");
                            else if (t.Code != team)
                                entry.Value.SwitchTeam(team);
                        }
                        team = (byte) Math.Max(team + 1, 2);
                    }
                    break;
                case State.TwoVsTwo:
                    List<string> userIdsTeam0 = new List<string>();
                    foreach (KeyValuePair<int, Photon.Realtime.Player> entry in PhotonNetwork.CurrentRoom.Players)
                    {
                        if (entry.Value.CustomProperties.ContainsKey(NetworkManager.masterTeamMateProp))
                        {
                            userIdsTeam0.Add(entry.Value.UserId);
                            userIdsTeam0.Add((string) entry.Value.CustomProperties[NetworkManager.masterTeamMateProp]);
                        }
                    }
                    if (userIdsTeam0.Count != 0)
                    {
                        int nbPlayerTeam0 = 0;
                        foreach (KeyValuePair<int, Photon.Realtime.Player> entry in PhotonNetwork.CurrentRoom.Players)
                        {
                            byte teamm = 0;
                            if (userIdsTeam0.Contains(entry.Value.UserId) && nbPlayerTeam0 < 2)
                            {
                                teamm = 1;
                                nbPlayerTeam0++;
                            }
                            else
                                teamm = 2;
                            if (!entry.Value.JoinTeam(teamm))
                            {
                                PhotonTeam t = entry.Value.GetPhotonTeam();
                                if (t == null)
                                    Debug.LogError("Join team failed");
                                else if (t.Code != teamm)
                                    entry.Value.SwitchTeam(teamm);
                            }
                        }
                    }
                    else
                    {
                        byte it = 0;
                        foreach (KeyValuePair<int, Photon.Realtime.Player> entry in PhotonNetwork.CurrentRoom.Players)
                        {
                            byte teammm = (byte) (it <= 1 ? 1 : 2);
                            if (!entry.Value.JoinTeam(teammm))
                            {
                                PhotonTeam t = entry.Value.GetPhotonTeam();
                                if (t == null)
                                    Debug.LogError("Join team failed");
                                else if (t.Code != teammm)
                                    entry.Value.SwitchTeam(teammm);
                            }
                            it++;
                        }
                    }

                    break;
            }
        }

        public State getMyCurrentRoomState()
        {
            return (State) myCustomProperties[stateProperty];
        }

        public RoomType getMyCurrentRoomType()
        {
            return (RoomType) myCustomProperties[roomTypeProperty];
        }

        public void synchRoomProperties()
        {
            myCustomProperties[stateProperty] = PhotonNetwork.CurrentRoom.CustomProperties[stateProperty];
            myCustomProperties[roomTypeProperty] = PhotonNetwork.CurrentRoom.CustomProperties[roomTypeProperty];
        }

        public RoomState RoomState { get {return roomState;} set {roomState = value;} }
    }
}