using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;

public class AIPlayer : ArenaPlayer
{
    public float rotationSpeed;
    
    // params
    private float reactionTimeInSecond = 2.0f;
    private float decisionThreshold = 0.6f;
    private float speedOfMovement = 0.01f;

    private System.Random rnd = new System.Random();
    private Dictionary<SpellCdEnum, CustomGesture> availableGestures = new Dictionary<SpellCdEnum, CustomGesture>();
    private Dictionary<SpellCdEnum, SpellCd> spellCds = new Dictionary<SpellCdEnum, SpellCd>
    {
        { SpellCdEnum.FireballLeft, new SpellCd(Fireball.FIREBALL_CD, "Fireball Left") },
        { SpellCdEnum.FireballRight, new SpellCd(Fireball.FIREBALL_CD, "Fireball Right") },
        { SpellCdEnum.Thunder, new SpellCd(Thunder.THUNDER_CD, "Thunder") },
        { SpellCdEnum.GrenadeRight, new SpellCd(Grenade.GRENADE_CD, "GrenadeRight") },
        { SpellCdEnum.GrenadeLeft, new SpellCd(Grenade.GRENADE_CD, "GrenadeLeft") },
        { SpellCdEnum.Cross, new SpellCd(Cross.CROSS_CD, "Cross") }
    };

    private List<SpellCdEnum> spellCdHandled = new List<SpellCdEnum>
        {SpellCdEnum.FireballLeft, SpellCdEnum.FireballRight, SpellCdEnum.Thunder,  SpellCdEnum.Cross};

    private float timeLastSpell = 0.0f;
    private SpellCdEnum spellSelected = SpellCdEnum.UNDEFINED;
    private List<CustomPoint> pointsLeftHand = new List<CustomPoint>();
    private List<CustomPoint> pointsLeftRealWorld = new List<CustomPoint>();
    private List<CustomPoint> pointsRightHand = new List<CustomPoint>();
    private List<CustomPoint> pointsRightRealWorld = new List<CustomPoint>();
    private int currentPointInMovementLeft = 0;
    private int currentPointInMovementRight = 0;
    private float timeCurrentPointLeftMovement = 0.0f;
    private float timeCurrentPointRightMovement = 0.0f;
    bool wasPerformingSpell = false;
    private Vector3 rightOriginLocalPosition;
    private Vector3 leftOriginLocalPosition;
    private SpellCreator spellCreator = new SpellCreator();

    // Start is called before the first frame update
    void Start()
    {
        if (photonView != null)
        {
            if (photonView.IsMine)
            {
                shield = PhotonNetwork.Instantiate("Shield", transform.position, transform.rotation);
            }
        }
        timeCurrentPointLeftMovement = speedOfMovement;
        timeCurrentPointRightMovement = speedOfMovement;
        rightOriginLocalPosition = rightHand.localPosition;
        leftOriginLocalPosition = leftHand.localPosition;
        spellCreator.Team = 1;
        setInfoCanvas();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            if (Main.gameStarted && !Main.gameEnded)
            {
                ArenaPlayer enemy = InformationCenter.getFirstPlayerOppositeTeam(team);
                updateAllSpellCds();
                if (performingSpell())
                {
                    moveLeftHandToNextPosition();
                    moveRightHandToNextPosition();
                    wasPerformingSpell = true;
                    timeLastSpell += Time.deltaTime;
                }
                else if (wasPerformingSpell)
                {
                    sendSpell(enemy);
                    wasPerformingSpell = false;
                }
                else if (shouldDoSomething())
                {
                    updateAvailableSpellList();
                    selectSpellFromList();
                    Debug.Log("select spell : " + spellSelected);
                    prepareGesture();
                }
                else
                {
                    updateRotationToFixEnemy(enemy, head);
                    updateRotationToFixEnemy(enemy, leftHand);
                    updateRotationToFixEnemy(enemy, rightHand);
                    timeLastSpell += Time.deltaTime;
                }
            }

        }
    }

    void resetHandPosition()
    {
        rightHand.localPosition = rightOriginLocalPosition;
        leftHand.localPosition = leftOriginLocalPosition;
    }

    void updateRotationToFixEnemy(ArenaPlayer enemy, Transform bodypart)
    {
        Vector3 direction = (enemy.getPosition() - bodypart.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        bodypart.rotation = Quaternion.Slerp(bodypart.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    void moveLeftHandToNextPosition()
    {
        timeCurrentPointLeftMovement += Time.deltaTime;
        if (timeCurrentPointLeftMovement >= speedOfMovement && currentPointInMovementLeft < pointsLeftHand.Count)
        {
            Quaternion rotation = isSpellTwoHanded(spellSelected) ? head.rotation : leftHand.rotation;
            leftHand.localPosition = leftOriginLocalPosition + rotation * pointsLeftHand[currentPointInMovementLeft++].toVector3();
            specialSpellPositionCorrection(leftHand);
            pointsLeftRealWorld.Add(new CustomPoint(leftHand.position, HandSideEnum.Left));
            timeCurrentPointLeftMovement = 0.0f;
        }
    }

    void moveRightHandToNextPosition()
    {
        timeCurrentPointRightMovement += Time.deltaTime;
        if (timeCurrentPointRightMovement >= speedOfMovement && currentPointInMovementRight < pointsRightHand.Count)
        {
            Quaternion rotation = isSpellTwoHanded(spellSelected) ? head.rotation : rightHand.rotation;
            rightHand.localPosition = rightOriginLocalPosition + rotation * pointsRightHand[currentPointInMovementRight++].toVector3();
            specialSpellPositionCorrection(rightHand);
            pointsRightRealWorld.Add(new CustomPoint(rightHand.position, HandSideEnum.Right));
            timeCurrentPointRightMovement = 0.0f;
        }
    }

    void specialSpellPositionCorrection(Transform hand)
    {
        hand.localPosition += new Vector3(0, 0.4f, 0);
        if (spellSelected == SpellCdEnum.Cross)
        {
            hand.localPosition += new Vector3(0, 0.5f, 0);
        }
    }


    bool performingSpell()
    {
        return currentPointInMovementLeft < pointsLeftHand.Count || currentPointInMovementRight < pointsRightHand.Count;
    }

    void prepareGesture()
    {
        pointsLeftRealWorld.Clear();
        pointsRightRealWorld.Clear();
        pointsLeftHand.Clear();
        pointsRightHand.Clear();
        currentPointInMovementLeft = 0;
        currentPointInMovementRight = 0;
        if (isSpellTwoHanded(spellSelected))
        {
            switch (spellSelected)
            {
                case SpellCdEnum.Thunder:
                    pointsLeftHand = getPointsFromSpellCd(SpellRecognition.ThunderLeft);
                    pointsRightHand = getPointsFromSpellCd(SpellRecognition.ThunderRight);
                    break;
                case SpellCdEnum.Cross:
                    pointsLeftHand = getPointsFromSpellCd(SpellRecognition.CrossLeft);
                    pointsRightHand = getPointsFromSpellCd(SpellRecognition.CrossRight);
                    break;
                default:
                    Debug.Log("Spell not implemented for the AI");
                    break;
            }
        }
        else
        {
            switch (spellSelected)
            {
                case SpellCdEnum.FireballLeft:
                    pointsLeftHand = new List<CustomPoint>(availableGestures[spellSelected].getCustomPoints());
                    break;
                case SpellCdEnum.FireballRight:
                    pointsRightHand = new List<CustomPoint>(availableGestures[spellSelected].getCustomPoints());
                    break;
            }
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
        Debug.Log("no spell found, snh");
        return new List<CustomPoint>();
    }

    private void updateAllSpellCds()
    {
        foreach (SpellCdEnum spell in SpellBook.SPELLS)
        {
            spellCds[spell].CurrentCd += Time.deltaTime;
        }
    }

    private bool isSpellTwoHanded(SpellCdEnum spellEnum)
    {
        return spellEnum == SpellCdEnum.Thunder || spellEnum == SpellCdEnum.Cross;
    }

    bool shouldDoSomething()
    {
        return timeLastSpell > reactionTimeInSecond && rnd.NextDouble() > decisionThreshold;
    }

    void updateAvailableSpellList()
    {
        availableGestures.Clear();
        foreach (CustomGesture gesture in CustomRecognizer.Candidates)
        {
            switch (gesture.getSpell())
            {
                case SpellRecognition.Thunder:
                    if (isSpellAvailable(SpellCdEnum.Thunder, Thunder.THUNDER_CD))
                        availableGestures.Add(SpellCdEnum.Thunder, gesture);
                    break;
                case SpellRecognition.Fireball:
                    if (isSpellAvailable(SpellCdEnum.FireballLeft, Fireball.FIREBALL_CD))
                        availableGestures.Add(SpellCdEnum.FireballLeft, gesture);
                    if (isSpellAvailable(SpellCdEnum.FireballRight, Fireball.FIREBALL_CD))
                        availableGestures.Add(SpellCdEnum.FireballRight, gesture);
                    break;
                case SpellRecognition.Cross:
                    if (isSpellAvailable(SpellCdEnum.Cross, Cross.CROSS_CD))
                        availableGestures.Add(SpellCdEnum.Cross, gesture);
                    break;
                // case SpellRecognition.GrenadeRight:
                // case SpellRecognition.GrenadeLeft:
            }
        }
    }

    public bool isSpellAvailable(SpellCdEnum spell, float spellCd)
    {
        return spellCds.ContainsKey(spell) && spellCds[spell].CurrentCd > spellCd;
    }

    void selectSpellFromList()
    {
        List<SpellCdEnum> l = new List<SpellCdEnum>(availableGestures.Keys);
        spellSelected = l[rnd.Next(l.Count)];
        spellCds[spellSelected].CurrentCd = 0.0f;
    }

    void sendSpell(ArenaPlayer enemy)
    {
        CustomGesture gesture = availableGestures[spellSelected];
        resetHandPosition();
        switch (spellSelected)
        {
            case SpellCdEnum.FireballLeft:
                spellCreator.createFireball(pointsLeftRealWorld);
                break;
            case SpellCdEnum.FireballRight:
                spellCreator.createFireball(pointsRightRealWorld);
                break;
            case SpellCdEnum.Cross:
                spellCreator.createCross(head, pointsLeftRealWorld, pointsRightRealWorld);
                break;
            case SpellCdEnum.Thunder:
                spellCreator.createThunder(enemy.getPosition());
                break;
        }
        timeLastSpell = 0.0f;
    }
}
