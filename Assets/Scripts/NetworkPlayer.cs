using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    private HandPresence leftHandPresence;
    private HandPresence rightHandPresence;

    private int health = GameSettings.PLAYER_HEALTH;
    private int id = -111;

    private GameObject shield;
    private InfoCanvas infoCanvas;

    private int team;
    private bool alive = true;

    private XRRig rig;

    private SpellBook spellBook = new SpellBook();

    // Start is called before the first frame update
    void Start()
    {
        rig = FindObjectOfType<XRRig>();
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

        leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<HandPresence>();
        leftHandPresence.setHandSide(HandSideEnum.Left);
        rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();
        rightHandPresence.setHandSide(HandSideEnum.Right);

        if (photonView != null)
        {
            // is this id correct ?
            id = photonView.Owner.ActorNumber;
            team = GameSettings.getTeamWithId(id);
            spellBook.Team = team;
            setInfoCanvas();
            if (photonView.IsMine)
            {
                shield = PhotonNetwork.Instantiate("Shield", headRig.transform.position, headRig.transform.rotation);
                StartPosition s = GameSettings.getStartPositionFromActorId(id);
                rig.transform.position = s.position;
                rig.transform.eulerAngles = s.rotation;
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }
            }
        }
    }

    public int getHealth()
    {
        return health;
    }
    
    void updateCdMapInfoCanvas()
    {
        infoCanvas.updateCdText(spellBook.SpellCds);
    }

    void setInfoCanvas()
    {
        if (id == 1)
        {
            infoCanvas = GameObject.Find("InfoCanvasP1").GetComponent<InfoCanvas>();
        }
        else
        {
            infoCanvas = GameObject.Find("InfoCanvasP2").GetComponent<InfoCanvas>();
        }

        if (infoCanvas == null)
        {
            Debug.Log("could not set infocanvas with id " + id);
        }
    }

    public void updateHealthInfoCanvas(int enemyHealth)
    {
        infoCanvas.updateHealth(health, enemyHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateHandAnimation(leftHandAnimator, leftHandPresence);
            UpdateHandAnimation(rightHandAnimator, rightHandPresence);

            spellBook.handleSpells(leftHandPresence, rightHandPresence);
            spellBook.handleShield(shield, leftHandPresence);
            spellBook.handleShield(shield, rightHandPresence);

            updateCdMapInfoCanvas();
        }
    }

    void UpdateHandAnimation(Animator handAnimator, HandPresence handPresence)
    {
        handAnimator.SetFloat("Trigger", handPresence.getTriggerValue());
        handAnimator.SetFloat("Grip", handPresence.getGripValue());
    }

    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }

    public void takeDamage(int damageAmount)
    {
        health = Mathf.Max(health - damageAmount, 0);
        CommunicationCenter.updateHealth();
    }

    public Vector3 getPosition()
    {
        return rig.transform.position;
    }

    public int getId()
    {
        return id;
    }

    public int Team { get { return team; } }
    public bool Alive { get { return alive; } }
}
