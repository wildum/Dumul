using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PausePopup : MonoBehaviour
{
    public Text popupText;
    float timePaused = 0.0f;

    void Update()
    {
        timePaused += Time.deltaTime;
        popupText.text = "Keep the button pressed for " + Mathf.CeilToInt(GameSettings.pauseTimePopup - timePaused) + " more seconds to return to lobby...";
        if (timePaused > GameSettings.pauseTimePopup)
        {
            Main.gameAborted = true;
        }
    }
}