using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellInfoPanel : MonoBehaviour
{
    public Text title;
    public Text damage;
    public Text cooldown;
    public Text description;

    public void setTitleText(string text)
    {
        title.text = text;
    }
    public void setDamageText(int damageValue)
    {
        damage.text = "DAMAGE : " + damageValue.ToString();
    }
    public void setCooldownText(float cd)
    {
        cooldown.text = "COOLDOWN : " + cd.ToString("F1");
    }
    public void setDescriptionText(string text)
    {
        description.text = text;
    }
}