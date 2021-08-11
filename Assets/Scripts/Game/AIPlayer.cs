using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;

public class AIPlayer : ArenaPlayer
{
    
    // params
    private AiPlayerParams config;

    const float distanceTrigger = 0.2f;

    private static System.Random rnd = new System.Random();
    private Dictionary<SpellCdEnum, CustomGesture> availableGestures = new Dictionary<SpellCdEnum, CustomGesture>();
    private Dictionary<SpellCdEnum, SpellCd> spellCds = new Dictionary<SpellCdEnum, SpellCd>
    {
        { SpellCdEnum.FireballLeft, new SpellCd(Fireball.FIREBALL_CD, "Fireball Left") },
        { SpellCdEnum.FireballRight, new SpellCd(Fireball.FIREBALL_CD, "Fireball Right") },
        { SpellCdEnum.Thunder, new SpellCd(Thunder.THUNDER_CD, "Thunder") },
        { SpellCdEnum.GrenadeRight, new SpellCd(Grenade.GRENADE_CD, "GrenadeRight") },
        { SpellCdEnum.GrenadeLeft, new SpellCd(Grenade.GRENADE_CD, "GrenadeLeft") },
        { SpellCdEnum.Cross, new SpellCd(Cross.CROSS_CD, "Cross") },
        { SpellCdEnum.DashLeft, new SpellCd(SpellBook.DASH_CD, "DashLeft") },
        { SpellCdEnum.DashRight, new SpellCd(SpellBook.DASH_CD, "DashRight") },
        { SpellCdEnum.Shield, new SpellCd(Shield.SHIELD_CD, "Shield") },
        { SpellCdEnum.LaserLeft, new SpellCd(Laser.LASER_CD, "LaserLeft") },
        { SpellCdEnum.LaserRight, new SpellCd(Laser.LASER_CD, "LaserRight") }
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
    private Vector3 originPosition;
    private Vector3 targetPosition;
    private SpellCreator spellCreator = new SpellCreator();
    private bool channelingLaser = false;
    private float timerLaser = 0f;

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
        loadConfig();
        timeCurrentPointLeftMovement = config.speedOfMovement;
        timeCurrentPointRightMovement = config.speedOfMovement;
        rightOriginLocalPosition = rightHand.localPosition;
        leftOriginLocalPosition = leftHand.localPosition;
        team = 1;
        spellCreator.Team = 1;
        setInfoCanvas();
        originPosition = transform.position;
        targetPosition = originPosition;
    }

    [PunRPC]
    public void setIdsRpc(int iid, int iidInTeam)
    {
        id = iid;
        idInTeam = iidInTeam;
        spellCreator.PlayerId = id;
        setPlayerMaterials(1, idInTeam);
    }

    public void setId(int id, int iidInTeam)
    {
        photonView.RPC("setIdsRpc", RpcTarget.All, id, iidInTeam);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            if (Main.gameStarted && !Main.gameEnded)
            {
                ArenaPlayer enemy = InformationCenter.getRandomPlayerOppositeTeam(team);
                updateAllSpellCds();
                if (channelingLaser)
                {
                    timerLaser += Time.deltaTime;
                    if (timerLaser > Laser.LASER_DURATION)
                    {
                        channelingLaser = false;
                        timerLaser = 0.0f;
                    }
                    moveAround();
                    updateRotationToFixEnemy(enemy, head);
                    updateRotationToFixEnemy(enemy, leftHand);
                    updateRotationToFixEnemy(enemy, rightHand);
                }
                else if (performingSpell())
                {
                    if (reachedFirstPosition(pointsLeftHand, currentPointInMovementLeft, leftHand, leftOriginLocalPosition))
                    {
                        moveLeftHandToNextPosition();
                    }
                    if (reachedFirstPosition(pointsRightHand, currentPointInMovementRight, rightHand, rightOriginLocalPosition))
                    {
                        moveRightHandToNextPosition();
                    }
                    wasPerformingSpell = true;
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
                    prepareGesture();
                }
                else
                {
                    moveAround();
                    resetHandPosition();
                    updateRotationToFixEnemy(enemy, head);
                    updateRotationToFixEnemy(enemy, leftHand);
                    updateRotationToFixEnemy(enemy, rightHand);
                    timeLastSpell += Time.deltaTime;
                }
            }

        }
    }

    void loadConfig()
    {
        AiDifficulty aiDiff = AiDifficulty.Easy;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(menu.RoomsHandler.aiDifficultyProperty))
        {
            aiDiff = (AiDifficulty) PhotonNetwork.CurrentRoom.CustomProperties[menu.RoomsHandler.aiDifficultyProperty];
            switch(aiDiff)
            {
                case AiDifficulty.Easy:
                    config = AiPlayerConfig.loadEasyConfig();
                    break;
                case AiDifficulty.Medium:
                    config = AiPlayerConfig.loadMediumConfig();
                    break;
                case AiDifficulty.Hard:
                    config = AiPlayerConfig.loadHardConfig();
                    break;
            }
        }
        else
        {
            Debug.LogError("AI diff not set, switchting to default easy");
        }
    }

    bool reachedFirstPosition(List<CustomPoint> points, int index, Transform hand, Vector3 startingPosition)
    {
        if (points.Count != 0 && index == 0)
        {
            Quaternion rotation = isSpellTwoHanded(spellSelected) ? head.rotation : hand.rotation;
            Vector3 p = computeNextHandPosition(rotation, points[0].toVector3());
            float d = Vector3.Distance(hand.localPosition, startingPosition + p + specialSpellPositionCorrection());
            if (d > distanceTrigger)
            {
                hand.localPosition = Vector3.Lerp(hand.localPosition, startingPosition + p + specialSpellPositionCorrection(), Time.deltaTime * config.handMovementSpeed);
                return false;
            }
        }
        return true;
    }

    float computeRndInterval(float min, float max)
    {
        return (float) rnd.NextDouble() * (max - min) + min;
    }

    void moveAround()
    {
        if (Vector3.Distance(transform.position, targetPosition) < distanceTrigger)
        {
            if (config.movementProba > rnd.NextDouble())
            {
                targetPosition = originPosition + new Vector3(computeRndInterval(-config.maxMovementRadiusDistance, config.maxMovementRadiusDistance),
                    computeRndInterval(-config.maxMovementHightRadiusDistance, config.maxMovementHightRadiusDistance),
                    computeRndInterval(-config.maxMovementRadiusDistance, config.maxMovementRadiusDistance));
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * (config.bodyMovementSpeed + (config.speedChange * computeRndInterval(-config.bodyMovementSpeed, config.bodyMovementSpeed))));
        }
    }

    void resetHandPosition()
    {
        rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightOriginLocalPosition, Time.deltaTime * config.handMovementSpeed);
        leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftOriginLocalPosition, Time.deltaTime * config.handMovementSpeed);
    }

    void updateRotationToFixEnemy(ArenaPlayer enemy, Transform bodypart)
    {
        Vector3 direction = enemy.getPosition() - bodypart.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        bodypart.rotation = Quaternion.Slerp(bodypart.rotation, rotation, Time.deltaTime * config.rotationSpeed);
    }

    void moveLeftHandToNextPosition()
    {
        timeCurrentPointLeftMovement += Time.deltaTime;
        if (timeCurrentPointLeftMovement >= config.speedOfMovement && currentPointInMovementLeft < pointsLeftHand.Count)
        {
            Quaternion rotation = isSpellTwoHanded(spellSelected) ? head.rotation : leftHand.rotation;
            leftHand.localPosition = leftOriginLocalPosition + computeNextHandPosition(rotation, pointsLeftHand[currentPointInMovementLeft++].toVector3());
            leftHand.localPosition += specialSpellPositionCorrection();
            pointsLeftRealWorld.Add(new CustomPoint(leftHand.position, HandSideEnum.Left));
            timeCurrentPointLeftMovement = 0.0f;
        }
    }

    void moveRightHandToNextPosition()
    {
        timeCurrentPointRightMovement += Time.deltaTime;
        if (timeCurrentPointRightMovement >= config.speedOfMovement && currentPointInMovementRight < pointsRightHand.Count)
        {
            Quaternion rotation = isSpellTwoHanded(spellSelected) ? head.rotation : rightHand.rotation;
            rightHand.localPosition = rightOriginLocalPosition + computeNextHandPosition(rotation, pointsRightHand[currentPointInMovementRight++].toVector3());
            rightHand.localPosition += specialSpellPositionCorrection();
            pointsRightRealWorld.Add(new CustomPoint(rightHand.position, HandSideEnum.Right));
            timeCurrentPointRightMovement = 0.0f;
        }
    }

    Vector3 computeNextHandPosition(Quaternion rotation, Vector3 position)
    {
        Vector3 newPos = Quaternion.Euler(0, rotation.eulerAngles.y, 0) * position;
        return Quaternion.Euler(rotation.eulerAngles.x, 0, rotation.eulerAngles.z) * newPos;
    }

    Vector3 specialSpellPositionCorrection()
    {
        Vector3 v = new Vector3(0, 0, 0);
        if (spellSelected == SpellCdEnum.Cross)
        {
            v.y = 0.8f;
            return v;
        }
        return v;
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
                case SpellCdEnum.GrenadeLeft:
                    pointsLeftHand = new List<CustomPoint>(availableGestures[spellSelected].getCustomPoints());
                    break;
                case SpellCdEnum.GrenadeRight:
                    pointsRightHand = new List<CustomPoint>(availableGestures[spellSelected].getCustomPoints());
                    break;
                case SpellCdEnum.LaserLeft:
                    pointsLeftHand = new List<CustomPoint>(availableGestures[spellSelected].getCustomPoints());
                    break;
                case SpellCdEnum.LaserRight:
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
        Debug.LogError("no spell found");
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
        return timeLastSpell > config.reactionTimeInSecond && rnd.NextDouble() > config.decisionThreshold;
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
                case SpellRecognition.GrenadeLeft:
                    if (isSpellAvailable(SpellCdEnum.GrenadeLeft, Grenade.GRENADE_CD))
                        availableGestures.Add(SpellCdEnum.GrenadeLeft, gesture);
                    break;
                case SpellRecognition.GrenadeRight:
                    if (isSpellAvailable(SpellCdEnum.GrenadeRight, Grenade.GRENADE_CD))
                        availableGestures.Add(SpellCdEnum.GrenadeRight, gesture);
                    break;
                case SpellRecognition.LaserLeft:
                    if (isSpellAvailable(SpellCdEnum.LaserLeft, Laser.LASER_CD))
                        availableGestures.Add(SpellCdEnum.LaserLeft, gesture);
                    break;
                case SpellRecognition.LaserRight:
                    if (isSpellAvailable(SpellCdEnum.LaserRight, Laser.LASER_CD))
                        availableGestures.Add(SpellCdEnum.LaserRight, gesture);
                    break;
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
    
    void handleGrenade(GameObject grenade, ArenaPlayer enemy)
    {
        grenade.GetComponent<Rigidbody>().useGravity = true;
        // aim above the enemy
        Vector3 direction = enemy.getPosition() - grenade.transform.position;
        float heightBonus = Vector3.Distance(grenade.transform.position, enemy.getPosition()) / 2.0f;
        direction.y += heightBonus;
        grenade.GetComponent<Rigidbody>().AddForce(direction * config.grenadeForce, ForceMode.Impulse);
    }

    void sendSpell(ArenaPlayer enemy)
    {
        CustomGesture gesture = availableGestures[spellSelected];
        GameObject grenade;
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
            case SpellCdEnum.GrenadeLeft:
                grenade = spellCreator.createGrenade(pointsLeftRealWorld);
                handleGrenade(grenade, enemy);
                break;
            case SpellCdEnum.GrenadeRight:
                grenade = spellCreator.createGrenade(pointsRightRealWorld);
                handleGrenade(grenade, enemy);
                break;
            case SpellCdEnum.LaserLeft:
                spellCreator.createLaserAI(leftHand, true);
                channelingLaser = true;
                break;
            case SpellCdEnum.LaserRight:
                spellCreator.createLaserAI(rightHand, false);
                channelingLaser = true;
                break;
        }
        timeLastSpell = 0.0f;
    }
}
