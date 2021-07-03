using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class SpellBook
{
    private const float MAX_TIME_TWO_HANDS_CAST = 2.0f;

    public const float DASH_CD = 6;
    public const float DASH_SPEED = 4.0f;

    public static List<SpellCdEnum> SPELLS = new List<SpellCdEnum> { SpellCdEnum.FireballLeft, SpellCdEnum.FireballRight, 
        SpellCdEnum.Thunder, SpellCdEnum.Cross,  SpellCdEnum.GrenadeLeft, SpellCdEnum.GrenadeRight, SpellCdEnum.DashLeft,  
        SpellCdEnum.DashRight, SpellCdEnum.Shield };

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
        {SpellRecognition.DashLeft, SpellEnum.DashLeft},
        {SpellRecognition.DashLeftOnePart, SpellEnum.DashLeft},
        {SpellRecognition.DashRight, SpellEnum.DashRight},
        {SpellRecognition.DashRightOnePart, SpellEnum.DashRight},
        {SpellRecognition.UNDEFINED, SpellEnum.UNDEFINED}
    };

    private Dictionary<SpellCdEnum, SpellCd> spellCds = new Dictionary<SpellCdEnum, SpellCd>
    {
        { SpellCdEnum.FireballLeft, new SpellCd(Fireball.FIREBALL_CD, "Fireball Left") },
        { SpellCdEnum.FireballRight, new SpellCd(Fireball.FIREBALL_CD, "Fireball Right") },
        { SpellCdEnum.Thunder, new SpellCd(Thunder.THUNDER_CD, "Thunder") },
        { SpellCdEnum.GrenadeRight, new SpellCd(Grenade.GRENADE_CD, "GrenadeRight") },
        { SpellCdEnum.GrenadeLeft, new SpellCd(Grenade.GRENADE_CD, "GrenadeLeft") },
        { SpellCdEnum.Cross, new SpellCd(Cross.CROSS_CD, "Cross") },
        { SpellCdEnum.DashRight, new SpellCd(DASH_CD, "DashRight") },
        { SpellCdEnum.DashLeft, new SpellCd(DASH_CD, "DashLeft") },
        { SpellCdEnum.Shield, new SpellCd(Shield.SHIELD_CD, "Shield")}
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
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(menu.RoomsHandler.stateProperty))
            {
                currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties[menu.RoomsHandler.stateProperty];
            }
            else
            {
                Debug.LogError("current state of the room not set");
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
            spellCds[spell].CurrentCd += currentState == State.Practice ? 999 : Time.deltaTime;
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
                        ArenaPlayer enemy = InformationCenter.getRelevantPlayerOppositeTeam(head.forward, head.position, team);
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
                    else if (spell == SpellEnum.DashLeft && isSpellAvailable(SpellCdEnum.DashLeft, DASH_CD))
                    {
                        spellcreator.createDash(team == 0 ? 2 : -2);
                        spellCds[SpellCdEnum.DashLeft].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                    else if (spell == SpellEnum.DashRight && isSpellAvailable(SpellCdEnum.DashRight, DASH_CD))
                    {
                        spellcreator.createDash(team == 0 ? -2 : 2);
                        spellCds[SpellCdEnum.DashRight].CurrentCd = 0.0f;
                        otherHand.resetTwoHandsLoadedSpell();
                    }
                }
                else
                {
                    hand.LoadedTwoHandsSpell = spellReco;
                    hand.LoadedPoints = new List<CustomPoint>(hand.getCustomRecognizerData().points);
                    Debug.Log("adding points");
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

    public void handleShield(GameObject shield, HandPresence left, HandPresence right)
    {
        Shield shieldObj = shield.GetComponent<Shield>();
        if ((left.activateShield() || right.activateShield())  && isSpellAvailable(SpellCdEnum.Shield, Shield.SHIELD_CD) && !shieldObj.Destroyed)
        {
            shieldObj.activate();
        }
        else if (!shieldObj.Destroyed)
        {
            shieldObj.deactivate();
        }
        else
        {
            shieldObj.Destroyed = false;
        }

        if (!shieldObj.isActive() && shieldObj.WasActive)
        {
            shieldObj.WasActive = false;
            spellCds[SpellCdEnum.Shield].CurrentCd = 0.0f;
        }
    }

    private bool isSpellTwoHanded(SpellEnum spellEnum)
    {
        return spellEnum == SpellEnum.Thunder || spellEnum == SpellEnum.Cross || spellEnum == SpellEnum.DashRight || spellEnum == SpellEnum.DashLeft;
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

    public void setIdPlayerSpellCreator(int id)
    {
        spellcreator.PlayerId = id;
    }

    public int Team { set { team = value; spellcreator.Team = value;} }
    public Dictionary<SpellCdEnum, SpellCd> SpellCds { get { return spellCds; } }
}