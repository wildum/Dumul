using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum EndGameStatus
{
    Victory,
    Defeat,
    Draw
}

public class InfoCanvas : MonoBehaviour
{
    public Text orangeHealth1;
    public Text orangeHealth2;
    public Text blueHealth1;
    public Text blueHealth2;
    public Text timer;
    public Text victory;
    public Text defeat;
    public Text draw;
    public CdGrid cdGrid;

    private const string tHealth = "HP : ";
    private const string tTimerBeforeStart = "Game starts in : ";

    private int minuteValue = 0;
    private int secondValue = 0;

    private void Start()
    {
        victory.gameObject.SetActive(false);
        defeat.gameObject.SetActive(false);
        draw.gameObject.SetActive(false);

        State currentState = State.Practice;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentState"))
        {
            currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties["currentState"];
        }
        else
        {
            Debug.Log("snh current state of the room not set");
        }

        if (currentState == State.Practice)
        {
            timer.gameObject.SetActive(false);
            orangeHealth1.gameObject.SetActive(false);
            orangeHealth2.gameObject.SetActive(false);
            blueHealth1.gameObject.SetActive(false);
            blueHealth2.gameObject.SetActive(false);
        }
        else if (currentState == State.OneVsOne || currentState == State.OneVsAI)
        {
            RectTransform rectTransformOrange = orangeHealth1.GetComponent<RectTransform>();
            rectTransformOrange.anchoredPosition = new Vector3(rectTransformOrange.anchoredPosition.x, rectTransformOrange.anchoredPosition.y - 30, 0);
            RectTransform rectTransformBlue = blueHealth1.GetComponent<RectTransform>();
            rectTransformBlue.anchoredPosition = new Vector3(rectTransformBlue.anchoredPosition.x, rectTransformBlue.anchoredPosition.y - 30, 0);
            orangeHealth2.gameObject.SetActive(false);
            blueHealth2.gameObject.SetActive(false);
        }
    }

    public void updateEndGameText(EndGameStatus endGameStatus)
    {
        if (endGameStatus == EndGameStatus.Victory)
        {
            victory.gameObject.SetActive(true);
        }
        else if(endGameStatus == EndGameStatus.Defeat)
        {
            defeat.gameObject.SetActive(true);
        }
        else if (endGameStatus == EndGameStatus.Draw)
        {
            draw.gameObject.SetActive(true);
        }
    }

    public void handleGameStarted(float timeSinceStart)
    {
        int timeInSeconds = Mathf.FloorToInt(GameSettings.gameTimer + GameSettings.timeBeforeStart - timeSinceStart);
        int valueDisplaySeconds = timeInSeconds % 60;
        if (secondValue != valueDisplaySeconds)
        {
            secondValue = valueDisplaySeconds;
            string t = Mathf.FloorToInt(timeInSeconds / 60) + " : " + (valueDisplaySeconds < 10 ? ("0" + valueDisplaySeconds.ToString()) : valueDisplaySeconds.ToString());
            timer.text = t;
        }
    }

    public void handleGameNotStarted(float timeSinceStart)
    {
        int tmpMinuteValue = Mathf.FloorToInt(GameSettings.timeBeforeStart - timeSinceStart);
        if (tmpMinuteValue != minuteValue)
        {
            minuteValue = tmpMinuteValue;
            string t = tTimerBeforeStart + minuteValue;
            timer.text = t;
        }
    }

    public void updateCdText(Dictionary<SpellCdEnum, SpellCd> spellsCd)
    {
        cdGrid.updateCdTextOnGrid(spellsCd);
    }

    public void updateHealth(int orangeHealth1Value, int orangeHealth2Value, int blueHealth1Value, int blueHealth2Value)
    {
        orangeHealth1.text = tHealth + orangeHealth1Value;
        orangeHealth2.text = tHealth + orangeHealth2Value;
        blueHealth1.text = tHealth + blueHealth1Value;
        blueHealth2.text = tHealth + blueHealth2Value;
    }

}
