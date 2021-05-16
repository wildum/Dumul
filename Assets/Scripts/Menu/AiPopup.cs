using System.Collections;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;

namespace menu
{
    public class AiPopup : MonoBehaviour
    {
        public RoomsHandler roomHandler;
        private int numberOfPlayers;

        public void easyButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Easy;
            startMode();
        }

        public void mediumButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Medium;
            startMode();
        }

        public void hardButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Hard;
            startMode();
        }

        private void startMode()
        {
            if (numberOfPlayers == 1)
            {
                roomHandler.startOneVsAI();
            }
            else
            {
                roomHandler.startTwoVsAI();
            }
            gameObject.SetActive(false);
        }

        public void returnButton()
        {
            gameObject.SetActive(false);
        }

        public int NumberOfPlayers { set { numberOfPlayers = value;}}
    }
}