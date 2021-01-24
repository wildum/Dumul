using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

static class SpellHandler
{

    public static List<SpellEnum> SPELLS = new List<SpellEnum>{SpellEnum.Fireball};

    public static void handleSpells(HandPresence leftHand, HandPresence rightHand, Dictionary<SpellEnum, float> cdMap)
    {
        bool consumed = handleTwoHandSpells(leftHand, rightHand, cdMap);

        if (!consumed)
        {
            handleOneHandSpell(leftHand, cdMap);
            handleOneHandSpell(rightHand, cdMap);
        }
        updateAllSpellCds(cdMap);
    }

    public static void updateAllSpellCds(Dictionary<SpellEnum, float> cdMap)
    {
        foreach (SpellEnum spell in SPELLS)
        {
            cdMap[spell] += Time.deltaTime;
        }
    }

    public static bool handleTwoHandSpells(HandPresence leftHand, HandPresence rightHand, Dictionary<SpellEnum, float> cdMap)
    {
        // return true when consumed
        return false;
    }

    public static void handleOneHandSpell(HandPresence hand, Dictionary<SpellEnum, float> cdMap)
    {
        ControlState gripState = hand.computeGripState();
        if (isJustPressed(gripState))
        {
            hand.startTrail();
        }
        else if (isJustReleased(gripState))
        {
            SpellEnum spell = SpellRecognizer.recognize(hand.getPoints());
            if (spell == SpellEnum.Fireball && isSpellAvailable(SpellEnum.Fireball, Fireball.CD_FIREBALL, cdMap))
            {
                createFireball(hand.transform);
                cdMap[SpellEnum.Fireball] = 0.0f;
            }
            hand.stopTrail();
        }
    }

    public static bool isSpellAvailable(SpellEnum spell, float spellCd, Dictionary<SpellEnum, float> cdMap)
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

    public static void createFireball(Transform t)
    {
        GameObject fireball = PhotonNetwork.Instantiate("Fireball", t.position, t.rotation);
        fireball.GetComponent<Rigidbody>().AddForce(t.forward * Fireball.SPEED, ForceMode.Force);
    }

}