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
