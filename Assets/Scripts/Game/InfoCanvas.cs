using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EndGameStatus
{
    Victory,
    Defeat,
    Draw
}

public class InfoCanvas : MonoBehaviour
{
    public Text myHealth;
    public Text enemyHealth;
    public Text timer;
    public Text victory;
    public Text defeat;
    public Text draw;
    public CdGrid cdGrid;

    private const string tHealth = "HP : ";
    private const string tOpp = "ENEMY : ";
    private const string tTimerBeforeStart = "Game starts in : ";

    private int minuteValue = 0;
    private int secondValue = 0;

    private void Start()
    {
        victory.gameObject.SetActive(false);
        defeat.gameObject.SetActive(false);
        draw.gameObject.SetActive(false);
        if (AppState.currentState == State.Pratice)
        {
            timer.gameObject.SetActive(false);
            myHealth.gameObject.SetActive(false);
            enemyHealth.gameObject.SetActive(false);
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

    public void updateHealth(int health, int oppHealth)
    {
        myHealth.text = tHealth + health;
        enemyHealth.text = tOpp + oppHealth;
    }

}
