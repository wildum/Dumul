using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : MonoBehaviour
{
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    private PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    private HandPresence leftHandPresence;
    private HandPresence rightHandPresence;

    private Dictionary<SpellEnum, float> cdMap = new Dictionary<SpellEnum, float>
        {{SpellEnum.Fireball, Fireball.CD_FIREBALL}}
    ;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XRRig rig = FindObjectOfType<XRRig>();
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

        leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<HandPresence>();
        rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();

        if (photonView.IsMine)
        {
            foreach (var item in GetComponentsInChildren<Renderer>())
	        {
                item.enabled = false;
	        }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateHandAnimation(leftHandAnimator, leftHandPresence);
            UpdateHandAnimation(rightHandAnimator, rightHandPresence);

            SpellHandler.handleSpells(leftHandPresence, rightHandPresence, cdMap);
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
}
