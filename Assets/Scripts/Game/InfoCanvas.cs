using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoCanvas : MonoBehaviour
{

    public Text myHealth;
    public Text enemyHealth;
    public Text timer;
    public CdGrid cdGrid;

    private const string tHealth = "HP : ";
    private const string tOpp = "ENEMY : ";
    private const string tTimerBeforeStart = "Game starts in : ";

    private int minuteValue = 0;
    private int secondValue = 0;

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
