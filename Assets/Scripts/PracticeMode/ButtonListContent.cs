using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

struct ButtonData
{
    public ButtonData(SpellEnum spell, string n, int idamage, float icd)
    {
        spellEnum = spell;
        name = n;
        damage = idamage;
        cd = icd;
        description = "";
        image = null;
    }
    public SpellEnum spellEnum;
    public string name;
    public int damage;
    public float cd;
    public string description;
    public Image image;
}

public class ButtonListContent : MonoBehaviour
{
    public HandPresence leftHand;
    public Scrollbar scrollbar;
    public SpellInfoPanel spellInfoPanel;
    public SpellPracticeMovement spellPracticeMovement;
    public GameObject ControlsPopup;
    public GameObject PracticeModeCanvas;
    public AudioSource Music;

    private List<ButtonData> spellsList = new List<ButtonData>
    {
        new ButtonData(SpellEnum.Cross, "Cross", Cross.CROSS_DAMAGE, Cross.CROSS_CD), 
        new ButtonData(SpellEnum.Fireball, "Fireball", Fireball.FIREBALL_DAMAGE, Fireball.FIREBALL_CD), 
        new ButtonData(SpellEnum.Grenade, "Grenade", Grenade.GRENADE_DAMAGE, Grenade.GRENADE_CD),
        new ButtonData(SpellEnum.Thunder, "Thunder", Thunder.THUNDER_DAMAGE, Thunder.THUNDER_CD)
    };
    private List<ButtonData> spellElements = new List<ButtonData>();

    private float timerSinceSwitch = 0;
    private const float SWITCH_TIMER = 4.0f;
    private int currentIndex = 0;
    private float momentum = 0;
    private bool startFirstMovement = false;
    private State currentState = State.Pratice;

    private float epsilon = 0.01f;

    private bool floatCompareZero(float value)
    {
        return value < epsilon && value > -epsilon;
    }

    void Start()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentState"))
        {
            currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties["currentState"];
        }
        else
        {
            Debug.Log("snh current state of the room not set");
        }

        if (currentState == State.Pratice)
        {
            spellInfoPanel.gameObject.SetActive(true);
            spellPracticeMovement.gameObject.SetActive(true);
            ControlsPopup.SetActive(true);
            PracticeModeCanvas.SetActive(true);
            Music.Play();

            TextAsset[] spellDescriptionXml = Resources.LoadAll<TextAsset>("SpellDescriptions/");
            Dictionary<SpellEnum, string> spellEnumToDescription = SpellDescriptionIO.ReadSpellDescriptionFromXML(spellDescriptionXml[0].text);
            GameObject spellTemplate = transform.GetChild(0).gameObject;
            GameObject g;
            for (int j = 0; j < spellsList.Count; j++)
            {
                ButtonData buttonData = spellsList[j];
                g = Instantiate(spellTemplate, transform);
                g.transform.GetChild(0).GetComponent<Text>().text = buttonData.name;
                Image i = g.GetComponent<Image>();
                i.GetComponent<Outline>().enabled = false;
                buttonData.image = i;
                buttonData.description = spellEnumToDescription[buttonData.spellEnum];
                spellElements.Add(buttonData);
            }
            Destroy(spellTemplate);
            if (spellElements.Count != 0)
            {
                spellElements[currentIndex].image.GetComponent<Outline>().enabled = true;
                setPanelData(spellElements[currentIndex]);
            }
        }
        else
        {
            spellInfoPanel.gameObject.SetActive(false);
            spellPracticeMovement.gameObject.SetActive(false);
            ControlsPopup.SetActive(false);
            PracticeModeCanvas.SetActive(false);
            Music.Stop();
        }
    }

    void Update()
    {
        if (currentState == State.Pratice)
        {
            if (!startFirstMovement && CustomRecognizer.candidatesLoaded)
            {
                startFirstMovement = true;
                spellPracticeMovement.setSpellMovement(spellElements[currentIndex].spellEnum);
            }
            float value = leftHand.JoystickData.y;
            if (!floatCompareZero(value))
            {
                timerSinceSwitch += Time.deltaTime * 10;
            }
            else
            {
                timerSinceSwitch = SWITCH_TIMER;
            }
            
            momentum = Mathf.Min(Mathf.Abs(value) * Time.deltaTime * 20 + momentum * 0.9f, 3.0f);
            if (!floatCompareZero(value) && (timerSinceSwitch + momentum) >= SWITCH_TIMER)
            {
                if (value < 0 && currentIndex < spellElements.Count-1)
                {
                    spellElements[currentIndex].image.GetComponent<Outline>().enabled = false;
                    spellElements[currentIndex+1].image.GetComponent<Outline>().enabled = true;
                    setPanelData(spellElements[currentIndex+1]);
                    spellPracticeMovement.setSpellMovement(spellElements[currentIndex+1].spellEnum);
                    currentIndex++;
                }
                else if (value > 0 && currentIndex > 0)
                {
                    spellElements[currentIndex].image.GetComponent<Outline>().enabled = false;
                    spellElements[currentIndex-1].image.GetComponent<Outline>().enabled = true;
                    setPanelData(spellElements[currentIndex-1]);
                    spellPracticeMovement.setSpellMovement(spellElements[currentIndex-1].spellEnum);
                    currentIndex--;
                }
                scrollbar.value = 1 - currentIndex / (spellElements.Count - 1.0f);
                timerSinceSwitch = 0;
            }
        }
    }

    void setPanelData(ButtonData buttonData)
    {
        spellInfoPanel.setTitleText(buttonData.name);
        spellInfoPanel.setDamageText(buttonData.damage);
        spellInfoPanel.setCooldownText(buttonData.cd);
        spellInfoPanel.setDescriptionText(buttonData.description);
    }

}
