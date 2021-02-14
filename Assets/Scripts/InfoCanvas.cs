using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoCanvas : MonoBehaviour
{

    public Text myHealth;
    public Text enemyHealth;
    public CdGrid cdGrid;

    private const string tHealth = "HP : ";
    private const string tOpp = "ENEMY : ";

    public void updateCdText(Dictionary<SpellCdEnum, float> cdMap)
    {
        cdGrid.updateCdTextOnGrid(cdMap);
    }

    public void updateWithMyHealth(int health)
    {
        myHealth.text = tHealth + health;
    }

    public void updateWithEnemyHealth(int health)
    {
        enemyHealth.text = tOpp + health;
    }

}
