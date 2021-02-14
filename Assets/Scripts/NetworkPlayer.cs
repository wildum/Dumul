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

    private PhotonView photonViewPlayer;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    private HandPresence leftHandPresence;
    private HandPresence rightHandPresence;

    private int health = GameSettings.PLAYER_HEALTH;
    private int id = -111;

    private GameObject shield;
    private InfoCanvas infoCanvas;

    private Dictionary<SpellCdEnum, float> cdMap = new Dictionary<SpellCdEnum, float>
        {{SpellCdEnum.FireballRight, Fireball.CD_FIREBALL}, {SpellCdEnum.FireballLeft, Fireball.CD_FIREBALL}}
    ;

    // Start is called before the first frame update
    void Start()
    {
        XRRig rig = FindObjectOfType<XRRig>();
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

        leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<HandPresence>();
        leftHandPresence.setHandSide(HandSideEnum.Left);
        rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();
        rightHandPresence.setHandSide(HandSideEnum.Right);

        if (photonView != null)
        {
            // assign position from here ?
            id = photonView.Owner.ActorNumber;
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
    
    void updateCdMapInfoCanvas()
    {
        infoCanvas.updateCdText(cdMap);
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

    void updateHealthInfoCanvas()
    {
        if (photonView.IsMine)
        {
            infoCanvas.updateWithMyHealth(health);
        }
        else
        {
            infoCanvas.updateWithEnemyHealth(health);
        }
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

            SpellHandler.handleSpells(leftHandPresence, rightHandPresence, cdMap, id);
            SpellHandler.handleShield(shield, leftHandPresence, photonView);
            SpellHandler.handleShield(shield, rightHandPresence, photonView);

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
        updateHealthInfoCanvas();
    }

    public int getId()
    {
        return id;
    }
}
