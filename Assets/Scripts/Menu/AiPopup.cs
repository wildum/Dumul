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
            startMode(AiDifficulty.Easy);
        }

        public void mediumButton()
        {
            startMode(AiDifficulty.Medium);
        }

        public void hardButton()
        {
            startMode(AiDifficulty.Hard);
        }

        private void startMode(AiDifficulty difficulty)
        {
            if (numberOfPlayers == 1)
            {
                roomHandler.startOneVsAI(difficulty);
            }
            else
            {
                roomHandler.startTwoVsAI(difficulty);
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