using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
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

    private Dictionary<SpellEnum, float> cdMap = new Dictionary<SpellEnum, float>
        {{SpellEnum.Fireball, Fireball.CD_FIREBALL}}
    ;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(id);
            stream.SendNext(health);
        }
        else
        {
            // Network player, receive data
            health = (int)stream.ReceiveNext();
            id = (int)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        XRRig rig = FindObjectOfType<XRRig>();
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

        leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<HandPresence>();
        rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();

        // create shield on head (not activated)
        shield = PhotonNetwork.Instantiate("Shield", headRig.transform.position, headRig.transform.rotation);
        shield.SetActive(false);

        if (photonView != null)
        {
            id = photonView.Owner.ActorNumber;
            Debug.Log(id);
            if (photonView.IsMine)
            {
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
            SpellHandler.handleShield(shield, leftHandPresence);
            SpellHandler.handleShield(shield, rightHandPresence);
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
