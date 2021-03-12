using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CdGrid : MonoBehaviour
{
    private Dictionary<SpellCdEnum, GameObject> cdSpellDict = new Dictionary<SpellCdEnum, GameObject>();
    private List<SpellCdEnum> cdDisplayed = new List<SpellCdEnum>();
    // Start is called before the first frame update
    void Start()
    {
        GameObject cdSpellTemplate = transform.GetChild(0).gameObject;
        GameObject g;
        foreach (SpellCdEnum spell in SpellBook.SPELLS)
        {
            g = Instantiate(cdSpellTemplate, transform);
            g.transform.GetChild(0).GetComponent<Slider>().value = 0;
            g.transform.GetChild(1).GetComponent<Text>().text ="";
            g.SetActive(false);
            cdSpellDict.Add(spell, g);
        }
        Destroy(cdSpellTemplate);
    }

    public void updateCdTextOnGrid(Dictionary<SpellCdEnum, SpellCd> spellsCd)
    {
        foreach(KeyValuePair<SpellCdEnum, SpellCd> entry in spellsCd)
        {
            GameObject g = cdSpellDict[entry.Key];
            if (entry.Value.CurrentCd < entry.Value.Cd)
            {
                g.transform.GetChild(0).GetComponent<Slider>().value = 1 - entry.Value.CurrentCd / entry.Value.Cd;
                g.transform.GetChild(1).GetComponent<Text>().text = entry.Value.Name;
                if(!cdDisplayed.Contains(entry.Key))
                {
                    cdDisplayed.Add(entry.Key);
                }
            }
            else if (g.activeSelf)
            {
                if (cdDisplayed.Contains(entry.Key))
                {
                    cdDisplayed.Remove(entry.Key);
                }
                g.SetActive(false);
            }
        }
        for (int i = 0; i < cdDisplayed.Count; i++)
        {
            cdSpellDict[cdDisplayed[i]].transform.SetSiblingIndex(i);
            cdSpellDict[cdDisplayed[i]].SetActive(true);
        }
    }

}
