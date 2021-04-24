using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellPracticeMovement : MonoBehaviour
{

    public PracticeHand handLeft;
    public PracticeHand handRight;

    bool currentSpellTwoHanded = false;
    float timerBetweenMovement = 0;
    const float timeBetweenMovement = 1.0f;
    HandSideEnum nextHandMovement = HandSideEnum.Right;

    private void Update()
    {
        if (handLeft.MovementFinished && handRight.MovementFinished)
        {
            timerBetweenMovement += Time.deltaTime;
            if (timerBetweenMovement > timeBetweenMovement)
            {
                if (currentSpellTwoHanded)
                {
                    handRight.gameObject.SetActive(true);
                    handLeft.gameObject.SetActive(true);
                    handRight.RestartMovement();
                    handLeft.RestartMovement();
                }
                else
                {
                    if (nextHandMovement == HandSideEnum.Right)
                    {
                        handRight.gameObject.SetActive(true);
                        handRight.RestartMovement();
                        nextHandMovement = HandSideEnum.Left;
                    }
                    else
                    {
                        handLeft.gameObject.SetActive(true);
                        handLeft.RestartMovement();
                        nextHandMovement = HandSideEnum.Right;
                    }
                }
            }
        }
    }

    public void setSpellMovement(SpellEnum spellEnum)
    {
        switch (spellEnum)
        {
            case SpellEnum.Fireball:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.Fireball));
                handRight.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.Fireball));
                currentSpellTwoHanded = false;
                break;
            case SpellEnum.Thunder:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.ThunderLeft));
                handRight.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.ThunderRight));
                currentSpellTwoHanded = true;
                break;
            case SpellEnum.Cross:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.CrossLeft));
                handRight.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.CrossRight));
                currentSpellTwoHanded = true;
                break;
            case SpellEnum.Grenade:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.GrenadeLeft));
                handRight.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.GrenadeRight));
                currentSpellTwoHanded = false;
                break;
        }
    }

    private List<CustomPoint> getPointsFromSpellCd(SpellRecognition spellReco)
    {
        foreach (CustomGesture gesture in CustomRecognizer.Candidates)
        {
            if (gesture.getSpell() == spellReco)
            {
                return new List<CustomPoint>(gesture.getCustomPoints());
            }
        }
        Debug.LogError("no spell found, snh");
        return new List<CustomPoint>();
    }

}