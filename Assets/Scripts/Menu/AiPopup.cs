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

        public void easyButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Easy;
            roomHandler.startOneVsAI();
        }

        public void mediumButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Medium;
            roomHandler.startOneVsAI();
        }

        public void hardButton()
        {
            AppState.currentAiDifficulty = AiDifficulty.Hard;
            roomHandler.startOneVsAI();
        }

        public void returnButton()
        {
            gameObject.SetActive(false);
        }
    }
}