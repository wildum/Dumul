using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class SpellBook
{
    private const float MAX_TIME_TWO_HANDS_CAST = 2.0f;

    public static List<SpellCdEnum> SPELLS = new List<SpellCdEnum> { SpellCdEnum.FireballLeft, SpellCdEnum.FireballRight, 
        SpellCdEnum.Thunder, SpellCdEnum.Cross,  SpellCdEnum.GrenadeLeft, SpellCdEnum.GrenadeRight };

    public static Dictionary<SpellRecognition, SpellEnum> recognitionToSpell = new Dictionary<SpellRecognition, SpellEnum>
    {
        {SpellRecognition.CrossLeft, SpellEnum.Cross},
        {SpellRecognition.CrossRight, SpellEnum.Cross},
        {SpellRecognition.Cross, SpellEnum.Cross},
        {SpellRecognition.Fireball, SpellEnum.Fireball},
        {SpellRecognition.Thunder, SpellEnum.Thunder},
        {SpellRecognition.ThunderRight, SpellEnum.Thunder},
        {SpellRecognition.ThunderLeft, SpellEnum.Thunder},
        {SpellRecognition.GrenadeLeft, SpellEnum.Grenade},
        {SpellRecognition.GrenadeRight, SpellEnum.Grenade},
        {SpellRecognition.UNDEFINED, SpellEnum.UNDEFINED}
    };

    private Dictionary<SpellCdEnum, SpellCd> spellCds = new Dictionary<SpellCdEnum, SpellCd>
    {
        { SpellCdEnum.FireballLeft, new SpellCd(Fireball.FIREBALL_CD, "Fireball Left") },
        { SpellCdEnum.FireballRight, new SpellCd(Fireball.FIREBALL_CD, "Fireball Right") },
        { SpellCdEnum.Thunder, new SpellCd(Thunder.THUNDER_CD, "Thunder") },
        { SpellCdEnum.GrenadeRight, new SpellCd(Grenade.GRENADE_CD, "GrenadeRight") },
        { SpellCdEnum.GrenadeLeft, new SpellCd(Grenade.GRENADE_CD, "GrenadeLeft") },
        { SpellCdEnum.Cross, new SpellCd(Cross.CROSS_CD, "Cross") }
    };

    private SpellCreator spellcreator = new SpellCreator();

    private int team;

    private State currentState;

    private bool init = false;

    private void initState()
    {
        if (!init)
        {
            init = true;
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentState"))
            {
                currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties["currentState"];
            }
            else
            {
                Debug.LogError("snh current state of the room not set");
            }
        }
    }

    public void handleSpells(Transform head, HandPresence leftHand, HandPresence rightHand)
    {
        initState();
        handleSpell(head, leftHand, rightHand);
        handleSpell(head, rightHand, leftHand);
        updateAllSpellCds();
        updateTwoHandsCd(leftHand);
        updateTwoHandsCd(rightHand);
    }

    private void updateTwoHandsCd(HandPresence hand)
    {
        if (hand.LoadedTwoHandsSpell != SpellRecognition.UNDEFINED)
        {
            if (hand.CurrentTimeTwoHandsSpell > MAX_TIME_TWO_HANDS_CAST)
            {
                hand.resetTwoHandsLoadedSpell();
            }
            else
            {
                hand.CurrentTimeTwoHandsSpell += Time.deltaTime;
            }
        }
    }

    private void updateAllSpellCds()
    {
        foreach (SpellCdEnum spell in SPELLS)
        {
            spellCds[spell].CurrentCd += currentState == State.Pratice ? 999 : Time.deltaTime;
        }
    }

    public void handleSpell(Transform head, HandPresence hand, HandPresence otherHand)
    {
        ControlState gripState = hand.computeGripState();
        if (isJustPressed(gripState))
        {
            hand.startTrail(TrailType.AttackSpell);
        }
        else if (isJustReleased(gripState))
        {
            CustomRecognizerData customData = hand.getCustomRecognizerData();
            // add points of other hand, usually empty except when loading a two hands movement
            if (hand.side == HandSideEnum.Left)
            {
                customData.points.AddRange(otherHand.LoadedPoints);
            }
            else
            {
                List<CustomPoint> tmp = new List<CustomPoint>(otherHand.LoadedPoints);
                tmp.AddRange(customData.points);
                customData.points = tmp;
            }
            SpellRecognition spellReco = SpellRecognizer.recognize(customData);
            SpellEnum spell = recognitionToSpell[spellReco];
            if (isSpellTwoHanded(spell))
            {
                if (recognitionToSpell[otherHand.LoadedTwoHandsSpell] == spell)
                {
                    if (spell == SpellEnum.Thunder && isSpellAvailable(SpellCdEnum.Thunder, Thunder.THUNDER_CD))
                    {
                        ArenaPlayer enemy = InformationCenter.getFirstPlayerOppositeTeam(team);
                        if (enemy != null)
                        {
                            spellcreator.createThunder(enemy.getPosition());
                            Debug.Log("casting thunder targeting : " + enemy.getPosition());
                        }
                        else
                        {
                            spellcreator.createThunder(new Vector3(0.0f, 0.0f, 0.0f));
                            Debug.Log("tried to cast thunder but not enemy around");
                        }
                        spellCds[SpellCdEnum.Thunder].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                    else if (spell == SpellEnum.Cross && isSpellAvailable(SpellCdEnum.Cross, Cross.CROSS_CD))
                    {
                        spellcreator.createCross(head, otherHand.LoadedPoints, hand.getCustomRecognizerData().points);
                        spellCds[SpellCdEnum.Cross].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                }
                else
                {
                    hand.LoadedTwoHandsSpell = spellReco;
                    hand.LoadedPoints = new List<CustomPoint>(hand.getCustomRecognizerData().points);
                }
            }
            else
            {
                SpellCdEnum fireBallType = hand.getHandSide() == HandSideEnum.Left ? SpellCdEnum.FireballLeft : SpellCdEnum.FireballRight;
                if (spellReco == SpellRecognition.Fireball && isSpellAvailable(fireBallType, Fireball.FIREBALL_CD))
                {
                    spellcreator.createFireball(hand.getCustomRecognizerData().points);
                    spellCds[fireBallType].CurrentCd = 0.0f;
                }
                else if (hand.getHandSide() == HandSideEnum.Left
                    && spellReco == SpellRecognition.GrenadeLeft
                    && isSpellAvailable(SpellCdEnum.GrenadeLeft, Grenade.GRENADE_CD))
                {
                    spellcreator.createGrenade(hand.getCustomRecognizerData().points);
                    spellCds[SpellCdEnum.GrenadeLeft].CurrentCd = 0.0f;
                }
                else if (hand.getHandSide() == HandSideEnum.Right
                    && spellReco == SpellRecognition.GrenadeRight
                    && isSpellAvailable(SpellCdEnum.GrenadeRight, Grenade.GRENADE_CD))
                {
                    spellcreator.createGrenade(hand.getCustomRecognizerData().points);
                    spellCds[SpellCdEnum.GrenadeRight].CurrentCd = 0.0f;
                }
            }
            hand.stopTrail(TrailType.AttackSpell);
        }
    }

    public void handleShield(GameObject shield, HandPresence hand)
    {
        // cannot use shield when using an attack spell already
        if (!hand.shielding())
        {
            ControlState triggerState = hand.computeTriggerState();
            if (isJustPressed(triggerState))
            {
                hand.startTrail(TrailType.DefenseSpell);
            }
            else if (isJustReleased(triggerState))
            {
                List<Vector3> shieldPoints = hand.getShieldPoints();
                Shield shieldObj = shield.GetComponent<Shield>();
                if (SpellRecognizer.recognizeShield(shieldPoints))
                {
                    moveShield(shield, shieldPoints);
                    shieldObj.photonView.RPC("reset", RpcTarget.All);
                    shieldObj.photonView.RPC("updateActive", RpcTarget.All, true);
                }
                else
                {
                    shieldObj.photonView.RPC("updateActive", RpcTarget.All, false);
                }
                hand.stopTrail(TrailType.DefenseSpell);
            }
        }
    }

    private bool isSpellTwoHanded(SpellEnum spellEnum)
    {
        return spellEnum == SpellEnum.Thunder || spellEnum == SpellEnum.Cross;
    }

    public bool isSpellAvailable(SpellCdEnum spell, float spellCd)
    {
        return spellCds.ContainsKey(spell) && spellCds[spell].CurrentCd > spellCd;
    }

    public bool isJustReleased(ControlState state)
    {
        return state == ControlState.JustReleased;
    }

    public bool isJustPressed(ControlState state)
    {
        return state == ControlState.JustPressed;
    }

    public void moveShield(GameObject shield, List<Vector3> shieldPoints)
    {
        if (shieldPoints.Count == 0)
            return;
        Vector3 smallest = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        Vector3 biggest = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
        Vector3 bigX = new Vector3(0,0,0);
        Vector3 smallX = new Vector3(0,0,0);
        Vector3 bigY = new Vector3(0,0,0);
        Vector3 smallY = new Vector3(0,0,0);
        Vector3 bigZ = new Vector3(0,0,0);
        Vector3 smallZ = new Vector3(0,0,0);
        float xcenter = 0;
        float ycenter = 0;
        float zcenter = 0;
        foreach (Vector3 p in shieldPoints)
        {
            if (p.x < smallest.x)
            {
                smallX = p;
                smallest.x = p.x;
            }
            if (p.x > biggest.x)
            {
                bigX = p;
                biggest.x = p.x;
            }
            if (p.y < smallest.y)
            {
                smallY = p;
                smallest.y = p.y;
            }
            if (p.y > biggest.y)
            {
                bigY = p;
                biggest.y = p.y;
            }
            if (p.z < smallest.z)
            {
                smallZ = p;
                smallest.z = p.z;
            }
            if (p.z > biggest.z)
            {
                bigZ = p;
                biggest.z = p.z;
            }
            xcenter += p.x;
            ycenter += p.y;
            zcenter += p.z;
        }
        Vector3 center = new Vector3(xcenter / shieldPoints.Count, ycenter / shieldPoints.Count, zcenter / shieldPoints.Count);
        shield.transform.position = center;
        Vector3 rotationX = bigX - center;
        rotationX.y = 0;
        Vector3 rotationZ = bigZ - center;
        rotationZ.y = 0;
        Vector3 rotationY = bigY - center;
        rotationY.x = 0;
        Vector3 rotationToUseForY = rotationX.x > rotationZ.z ? rotationX : rotationZ;
        float yAngle = Vector3.SignedAngle(Vector3.right, rotationToUseForY, Vector3.up);
        if (yAngle < 0)
        {
            yAngle += 360;
        }
        float xAngle = Vector3.SignedAngle(Vector3.up, rotationY, Vector3.right);
        if (xAngle < 0)
        {
            xAngle += 360;
        }
        // Debug.Log("Vector x : " + rotationX.x + " " + rotationX.z);
        // Debug.Log("Vector y : " + rotationY.y + " " + rotationY.z);
        // Debug.Log("Vector z : " + rotationZ.x + " " + rotationZ.z);
        Debug.Log(xAngle + " " + yAngle);
        shield.transform.eulerAngles = new Vector3(xAngle, yAngle, 0);

        // scaling
        float xValue = Mathf.Abs(biggest.x - smallest.x);
        float yValue = Mathf.Abs(biggest.y - smallest.y);
        float zValue = Mathf.Abs(biggest.z - smallest.z);
        float xscale = 0;
        float yscale = 0;
        if (xValue > zValue && yValue > zValue)
        {
            xscale = xValue;
            yscale = yValue;
        }
        else if (xValue < zValue && yValue > zValue)
        {
            xscale = zValue;
            yscale = yValue;
        }
        else if (yValue < zValue && xValue > zValue)
        {
            xscale = xValue;
            yscale = zValue;
        }
        else
        {
            if (xValue < yValue)
            {
                xscale = zValue;
                yscale = yValue;
            }
            else
            {
                xscale = xValue;
                yscale = zValue;
            }
        }
        shield.transform.localScale = new Vector3(xscale, yscale, shield.transform.localScale.z);
    }

    public int Team { set { team = value; spellcreator.Team = value;} }
    public Dictionary<SpellCdEnum, SpellCd> SpellCds { get { return spellCds; } }
}