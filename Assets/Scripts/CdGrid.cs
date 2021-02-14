using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CdGrid : MonoBehaviour
{
    private Dictionary<SpellCdEnum, GameObject> cdSpellDict = new Dictionary<SpellCdEnum, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GameObject cdSpellTemplate = transform.GetChild(0).gameObject;
        GameObject g;
        foreach (SpellCdEnum spell in SpellHandler.SPELLS)
        {
            g = Instantiate(cdSpellTemplate, transform);
            g.transform.GetChild(0).GetComponent<Slider>().value = 0;
            g.transform.GetChild(1).GetComponent<Text>().text ="";
            g.SetActive(false);
            cdSpellDict.Add(spell, g);
        }
        Destroy(cdSpellTemplate);
    }

    public void updateCdTextOnGrid(Dictionary<SpellCdEnum, float> cdMap)
    {
        foreach (KeyValuePair<SpellCdEnum, float> entry in cdMap)
        {
            float cdMaxSpell = SpellHandler.SPELLS_CD_MAP[entry.Key];
            GameObject g = cdSpellDict[entry.Key];
            if (entry.Value < cdMaxSpell)
            {
                g.transform.GetChild(0).GetComponent<Slider>().value = 1 - entry.Value / cdMaxSpell;
                g.transform.GetChild(1).GetComponent<Text>().text = SpellHandler.SPELLS_CD_String[entry.Key];
                g.SetActive(true);
            }
            else if (g.activeSelf)
            {
                g.SetActive(false);
            }
        }
    }

}
