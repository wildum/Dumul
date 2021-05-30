using System.Collections;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;

namespace menu
{
    public class ButtonManager : MonoBehaviour, IOnEventCallback
    {
        public RoomsHandler roomHandler;
        public Text lobbyText;
        public AiPopup aiPopup;

        void Start()
        {
            aiPopup.gameObject.SetActive(false);
            lobbyText.gameObject.SetActive(false);
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == NetworkManager.StartGameTextEventCode)
            {
                lobbyText.gameObject.SetActive(true);
                lobbyText.text = "The game is about to start";
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void updateButtonsWaitingRoom()
        {
            if (PhotonNetwork.IsConnected)
            {
                lobbyText.gameObject.SetActive(false);
                if (PhotonNetwork.IsMasterClient)
                {
                    switch (PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        case 1:
                            buttonConfigOnePlayer();
                            break;
                        case 2:
                            buttonConfigTwoPlayers();
                            break;
                        case 3:
                            buttonConfigThreePlayer();
                            break;
                        case 4:
                            buttonConfigFourPlayer();
                            break;
                        default:
                            buttonConfigNotMaster();
                            break;
                    }
                }
                else
                {
                    buttonConfigNotMaster();
                }
            }
            else
            {
                buttonConfigOffline();
            }
        }

        public void buttonInLobbyConfig(State state)
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vAI", false);
            updateButtonInteractableByName("1vAI", false);
            updateButtonInteractableByName("Practice", false);
            updateButtonInteractableByName("QuitLobby", true);
            lobbyText.gameObject.SetActive(false);
            switch (state)
            {
                case State.OneVsOne:
                    lobbyText.gameObject.SetActive(true);
                    lobbyText.text = "Lobby 1v1, waiting for an opponent";
                    break;
                case State.TwoVsTwo:
                    lobbyText.gameObject.SetActive(true);
                    lobbyText.text = "Lobby 2v2, waiting for players";
                    break;
                case State.TwoVsAI:
                    lobbyText.gameObject.SetActive(true);
                    lobbyText.text = "Lobby 2vAI, waiting for an ally";
                    break;
            }
        }

        private void buttonConfigOffline()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vAI", false);
            updateButtonInteractableByName("1vAI", true);
            updateButtonInteractableByName("Practice", true);
            updateButtonInteractableByName("QuitLobby", false);
        }
        private void buttonConfigTwoPlayers()
        {
            updateButtonInteractableByName("1v1", true);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vAI", true);
            updateButtonInteractableByName("1vAI", false);
            updateButtonInteractableByName("Practice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }
        private void buttonConfigOnePlayer()
        {
            updateButtonInteractableByName("1v1", true);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vAI", true);
            updateButtonInteractableByName("1vAI", true);
            updateButtonInteractableByName("Practice", true);
            updateButtonInteractableByName("QuitLobby", false);
        }
        private void buttonConfigThreePlayer()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vAI", false);
            updateButtonInteractableByName("1vAI", false);
            updateButtonInteractableByName("Practice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }
        private void buttonConfigFourPlayer()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vAI", false);
            updateButtonInteractableByName("1vAI", false);
            updateButtonInteractableByName("Practice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }

        private void buttonConfigNotMaster()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vAI", false);
            updateButtonInteractableByName("1vAI", false);
            updateButtonInteractableByName("Practice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }

        private void updateButtonInteractableByName(string name, bool status)
        {
            GameObject g = gameObject.transform.Find(name).gameObject;
            g.GetComponent<Button>().interactable = status;
        }

        private void updateButtonSetActiveByName(string name, bool status)
        {
            gameObject.transform.Find(name).gameObject.SetActive(status);
        }

        public void loadOneVsOne()
        {
            roomHandler.handle1v1Matchmaking();
        }

        public void loadTwoVsTwo()
        {
            roomHandler.handle2v2Matchmaking();
        }

        public void loadPractice()
        {
            roomHandler.startPractice();
        }

        public void loadOneVsAI()
        {
            aiPopup.NumberOfPlayers = 1;
            aiPopup.gameObject.SetActive(true);
        }

        public void loadTwoVsIA()
        {
            aiPopup.NumberOfPlayers = 2;
            aiPopup.gameObject.SetActive(true);
        }

        public void quitLobby()
        {
            roomHandler.RoomState = RoomState.Chill;
            PhotonNetwork.LeaveRoom();
        }

        public void quitGame()
        {
            Application.Quit();
        }
    }
}