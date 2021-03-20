using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class SpellBook
{
    private const float MAX_TIME_TWO_HANDS_CAST = 2.0f;

    public static List<SpellCdEnum> SPELLS = new List<SpellCdEnum> { SpellCdEnum.FireballLeft, SpellCdEnum.FireballRight, SpellCdEnum.Thunder, SpellCdEnum.Cross };

    private static Dictionary<SpellRecognition, SpellEnum> recognitionToSpell = new Dictionary<SpellRecognition, SpellEnum>
    {
        {SpellRecognition.CrossLeft, SpellEnum.Cross},
        {SpellRecognition.CrossRight, SpellEnum.Cross},
        {SpellRecognition.Fireball, SpellEnum.Fireball},
        {SpellRecognition.Thunder, SpellEnum.Thunder},
        {SpellRecognition.UNDEFINED, SpellEnum.UNDEFINED}
    };


    private Dictionary<SpellCdEnum, SpellCd> spellCds = new Dictionary<SpellCdEnum, SpellCd>
    {
        { SpellCdEnum.FireballLeft, new SpellCd(Fireball.FIREBALL_CD, "Fireball Left") },
        { SpellCdEnum.FireballRight, new SpellCd(Fireball.FIREBALL_CD, "Fireball Right") },
        { SpellCdEnum.Thunder, new SpellCd(Thunder.THUNDER_CD, "Thunder") },
        { SpellCdEnum.Cross, new SpellCd(Cross.CROSS_CD, "Cross") }
    };

    private int team;

    public void handleSpells(Transform head, HandPresence leftHand, HandPresence rightHand)
    {
        handleOneHandSpell(head, leftHand, rightHand);
        handleOneHandSpell(head, rightHand, leftHand);
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
            spellCds[spell].CurrentCd += Time.deltaTime;
        }
    }

    public void handleOneHandSpell(Transform head, HandPresence hand, HandPresence otherHand)
    {
        ControlState gripState = hand.computeGripState();
        if (isJustPressed(gripState))
        {
            hand.startTrail(TrailType.AttackSpell);
        }
        else if (isJustReleased(gripState))
        {
            SpellRecognition spellReco = SpellRecognizer.recognize(hand.getCustomRecognizerData());
            SpellEnum spell = recognitionToSpell[spellReco];
            if (isSpellTwoHanded(spell))
            {
                if (recognitionToSpell[otherHand.LoadedTwoHandsSpell] == spell)
                {
                    if (spell == SpellEnum.Thunder && isSpellAvailable(SpellCdEnum.Thunder, Thunder.THUNDER_CD))
                    {
                        NetworkPlayer enemy = InformationCenter.getFirstPlayerOppositeTeam(team);
                        if (enemy != null)
                        {
                            createThunder(enemy.getPosition());
                            Debug.Log("casting thunder targeting : " + enemy.getPosition());
                        }
                        else
                        {
                            createThunder(new Vector3(0.0f, 0.0f, 0.0f));
                            Debug.Log("tried to cast thunder but not enemy around");
                        }
                        spellCds[SpellCdEnum.Thunder].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                    else if (spell == SpellEnum.Cross && isSpellAvailable(SpellCdEnum.Cross, Cross.CROSS_CD))
                    {
                        createCross(head, otherHand.LoadedPoints, hand.getCustomRecognizerData().points);
                        spellCds[SpellCdEnum.Cross].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                }
                else
                {
                    hand.LoadedTwoHandsSpell = spellReco;
                    hand.LoadedPoints = new List<Vector3>(hand.getCustomRecognizerData().points);
                }
            }
            else
            {
                SpellCdEnum fireBallType = hand.getHandSide() == HandSideEnum.Left ? SpellCdEnum.FireballLeft : SpellCdEnum.FireballRight;
                if (spell == SpellEnum.Fireball && isSpellAvailable(fireBallType, Fireball.FIREBALL_CD))
                {
                    createFireball(hand.transform, team);
                    spellCds[fireBallType].CurrentCd = 0.0f;
                }
            }
            hand.stopTrail(TrailType.AttackSpell);
        }
    }

    private void createCross(Transform head, List<Vector3> p1, List<Vector3> p2)
    {
        Vector3 position = Tools.foundClosestMiddlePointBetweenTwoLists(p1, p2);
        position.y += 0.5f;
        // spawn it a little further from the player to avoid self collision
        position = position + (head.forward / 2.0f);
        Quaternion quaternion = head.rotation;
        quaternion.eulerAngles = new Vector3(90, quaternion.eulerAngles.y, 0);
        GameObject cross = PhotonNetwork.Instantiate("Cross", position, quaternion);
        cross.GetComponent<Rigidbody>().AddForce(head.forward * Cross.CROSS_SPEED, ForceMode.VelocityChange);
        cross.GetComponent<Cross>().setTeam(team);
    }

    public void handleShield(GameObject shield, HandPresence hand)
    {
        // cannot use shield when using an attack spell already
        if (!hand.gripPressing())
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

    private  void createFireball(Transform t, int team)
    {
        GameObject fireball = PhotonNetwork.Instantiate("Fireball", t.position, t.rotation);
        fireball.GetComponent<Rigidbody>().AddForce(t.forward * Fireball.FIREBALL_SPEED, ForceMode.Force);
        fireball.GetComponent<Fireball>().setTeam(team);
    }

    private void createThunder(Vector3 enemyPosition)
    {
        Vector3 position = new Vector3(enemyPosition.x, Thunder.yStartingPosition, enemyPosition.z);
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0,0,180);
        PhotonNetwork.Instantiate("Thunder", position, rotation);
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

    public int Team { set { team = value; } }
    public Dictionary<SpellCdEnum, SpellCd> SpellCds { get { return spellCds; } }
}