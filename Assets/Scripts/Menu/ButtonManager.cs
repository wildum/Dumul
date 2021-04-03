using System.Collections;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;

namespace menu
{
    public class ButtonManager : MonoBehaviour
    {
        public RoomsHandler roomHandler;
        public Text queueText;

        void Start()
        {
            queueText.gameObject.SetActive(false);
            updateButtonSetActiveByName("CancelQueue", false);
        }

        public void updateButtonsStatus()
        {
            if (PhotonNetwork.IsConnected)
            {
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

        private void buttonConfigOffline()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vIA", false);
            updateButtonInteractableByName("1vIA", true);
            updateButtonInteractableByName("Pratice", true);
            updateButtonInteractableByName("QuitLobby", false);
        }
        private void buttonConfigTwoPlayers()
        {
            updateButtonInteractableByName("1v1", true);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vIA", true);
            updateButtonInteractableByName("1vIA", false);
            updateButtonInteractableByName("Pratice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }
        private void buttonConfigOnePlayer()
        {
            updateButtonInteractableByName("1v1", true);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vIA", true);
            updateButtonInteractableByName("1vIA", true);
            updateButtonInteractableByName("Pratice", true);
            updateButtonInteractableByName("QuitLobby", false);
        }
        private void buttonConfigThreePlayer()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vIA", false);
            updateButtonInteractableByName("1vIA", false);
            updateButtonInteractableByName("Pratice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }
        private void buttonConfigFourPlayer()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", true);
            updateButtonInteractableByName("2vIA", false);
            updateButtonInteractableByName("1vIA", false);
            updateButtonInteractableByName("Pratice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }
        private void buttonConfigNotMaster()
        {
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vIA", false);
            updateButtonInteractableByName("1vIA", false);
            updateButtonInteractableByName("Pratice", false);
            updateButtonInteractableByName("QuitLobby", true);
        }

        private void buttonConfigDuringQueue()
        {
            updateButtonSetActiveByName("CancelQueue", true);
            queueText.gameObject.SetActive(true);
            updateButtonInteractableByName("1v1", false);
            updateButtonInteractableByName("2v2", false);
            updateButtonInteractableByName("2vIA", false);
            updateButtonInteractableByName("1vIA", false);
            updateButtonInteractableByName("Pratice", false);
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
            buttonConfigDuringQueue();
            roomHandler.handle1v1Matchmaking();
        }

        public void loadPratice()
        {
            roomHandler.startPratice();
        }

        public void loadOneVsAI()
        {
            roomHandler.startOneVsAI();
        }

        public void cancelQueue()
        {
            updateButtonsStatus();
            updateButtonSetActiveByName("CancelQueue", false);
            queueText.gameObject.SetActive(false);
            roomHandler.resetRoom();
        }

        public void quitLobby()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void quitGame()
        {
            Application.Quit();
        }
    }
}