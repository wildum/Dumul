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
                handLeft.loadPoints(spellEnum, getPointsFromSpellCdWithSide(SpellRecognition.Thunder, HandSideEnum.Left));
                handRight.loadPoints(spellEnum, getPointsFromSpellCdWithSide(SpellRecognition.Thunder, HandSideEnum.Right));
                currentSpellTwoHanded = true;
                break;
            case SpellEnum.Cross:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCdWithSide(SpellRecognition.Cross, HandSideEnum.Left));
                handRight.loadPoints(spellEnum, getPointsFromSpellCdWithSide(SpellRecognition.Cross, HandSideEnum.Right));
                currentSpellTwoHanded = true;
                break;
            case SpellEnum.Grenade:
                handLeft.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.GrenadeLeft));
                handRight.loadPoints(spellEnum, getPointsFromSpellCd(SpellRecognition.GrenadeRight));
                currentSpellTwoHanded = false;
                break;
        }
    }

    private List<CustomPoint> getPointsFromSpellCdWithSide(SpellRecognition spellReco, HandSideEnum side)
    {
        foreach (CustomGesture gesture in CustomRecognizer.Candidates)
        {
            if (gesture.getSpell() == spellReco)
            {
                List<CustomPoint> l = new List<CustomPoint>();
                foreach (CustomPoint p in gesture.getCustomPoints())
                {
                    if (side == p.side)
                    {
                        l.Add(p);
                    }
                }
                return l;
            }
        }
        Debug.LogError("no spell found");
        return new List<CustomPoint>();
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
        Debug.LogError("no spell found");
        return new List<CustomPoint>();
    }

}