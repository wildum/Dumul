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

    private int health = 1000;
    private int id = -111;

    private GameObject shield;

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
            if (photonView.IsMine)
            {
                shield = PhotonNetwork.Instantiate("Shield", headRig.transform.position, headRig.transform.rotation);
                Debug.Log("My id is : " + id);
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
    }

    public int getId()
    {
        return id;
    }
}
