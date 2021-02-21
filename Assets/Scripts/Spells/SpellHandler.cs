using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

static class SpellHandler
{

    public static List<SpellCdEnum> SPELLS = new List<SpellCdEnum> { SpellCdEnum.FireballLeft, SpellCdEnum.FireballRight };

    public static Dictionary<SpellCdEnum, float> SPELLS_CD_MAP = new Dictionary<SpellCdEnum, float> {
        { SpellCdEnum.FireballLeft, Fireball.CD_FIREBALL },
        { SpellCdEnum.FireballRight, Fireball.CD_FIREBALL }
    };

    public static Dictionary<SpellCdEnum, string> SPELLS_CD_String = new Dictionary<SpellCdEnum, string> {
        { SpellCdEnum.FireballLeft, "Fireball Left" },
        { SpellCdEnum.FireballRight, "Fireball Right" }
    };

    public static void handleSpells(HandPresence leftHand, HandPresence rightHand, Dictionary<SpellCdEnum, float> cdMap, int ownerId)
    {
        bool consumed = handleTwoHandSpells(leftHand, rightHand, cdMap, ownerId);

        if (!consumed)
        {
            handleOneHandSpell(leftHand, cdMap, ownerId);
            handleOneHandSpell(rightHand, cdMap, ownerId);
        }
        updateAllSpellCds(cdMap);
    }

    public static void updateAllSpellCds(Dictionary<SpellCdEnum, float> cdMap)
    {
        foreach (SpellCdEnum spell in SPELLS)
        {
            cdMap[spell] += Time.deltaTime;
        }
    }

    public static bool handleTwoHandSpells(HandPresence leftHand, HandPresence rightHand, Dictionary<SpellCdEnum, float> cdMap, int ownerId)
    {
        // return true when consumed
        return false;
    }

    public static void handleOneHandSpell(HandPresence hand, Dictionary<SpellCdEnum, float> cdMap, int ownerId)
    {
        ControlState gripState = hand.computeGripState();
        if (isJustPressed(gripState))
        {
            hand.startTrail(TrailType.AttackSpell);
        }
        else if (isJustReleased(gripState))
        {
            SpellEnum spell = SpellRecognizer.recognize(hand.getPoints(), hand.getCustomRecognizerData());
            SpellCdEnum fireBallType = hand.getHandSide() == HandSideEnum.Left ? SpellCdEnum.FireballLeft : SpellCdEnum.FireballRight;
            if (spell == SpellEnum.Fireball && isSpellAvailable(fireBallType, Fireball.CD_FIREBALL, cdMap))
            {
                createFireball(hand.transform, ownerId);
                cdMap[fireBallType] = 0.0f;
            }
            hand.stopTrail(TrailType.AttackSpell);
        }
    }

    public static void handleShield(GameObject shield, HandPresence hand, PhotonView photonView)
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

    public static bool isSpellAvailable(SpellCdEnum spell, float spellCd, Dictionary<SpellCdEnum, float> cdMap)
    {
        return cdMap.ContainsKey(spell) && cdMap[spell] > spellCd;
    }

    public static bool isJustReleased(ControlState state)
    {
        return state == ControlState.JustReleased;
    }

    public static bool isJustPressed(ControlState state)
    {
        return state == ControlState.JustPressed;
    }

    public static void createFireball(Transform t, int ownerId)
    {
        GameObject fireball = PhotonNetwork.Instantiate("Fireball", t.position, t.rotation);
        fireball.GetComponent<Rigidbody>().AddForce(t.forward * Fireball.SPEED, ForceMode.Force);
        fireball.GetComponent<Fireball>().setOwnerId(ownerId);
    }

    public static void moveShield(GameObject shield, List<Vector3> shieldPoints)
    {
        Vector3 smallest = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        Vector3 biggest = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
        Vector3 bigX = new Vector3(0,0,0);
        Vector3 smallX = new Vector3(0,0,0);
        Vector3 bigY = new Vector3(0,0,0);
        Vector3 smallY = new Vector3(0,0,0);
        Vector3 bigZ = new Vector3(0,0,0);
        Vector3 smallZ = new Vector3(0,0,0);
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
        }
        shield.transform.position = new Vector3(Tools.middle(biggest.x, smallest.x), Tools.middle(biggest.y, smallest.y), Tools.middle(biggest.z, smallest.z));
        Vector3 rotationX = bigX - smallX;
        rotationX.y = 0;
        Vector3 rotationZ = bigZ - smallZ;
        rotationZ.y = 0;
        Vector3 rotationY = bigY - smallY;
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
        // Debug.Log(xAngle + " " + yAngle);
        shield.transform.eulerAngles = new Vector3(xAngle, yAngle, 0);
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

}