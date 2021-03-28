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

    private GameObject shield;
    private InfoCanvas infoCanvas;

    private int team = -1;
    private bool teamAssigned = false;
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
        rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();

        if (photonView != null)
        {
            if (photonView.IsMine)
            {
                shield = PhotonNetwork.Instantiate("Shield", headRig.transform.position, headRig.transform.rotation);
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }
            }
        }
    }

    [PunRPC]
    public void setTeamRPC(int teamId)
    {
        team = teamId;
        spellBook.Team = team;
        leftHandPresence.Team = team;
        rightHandPresence.Team = team;
        setInfoCanvas();
        if (photonView.IsMine)
        {
            StartPosition s = GameSettings.getStartPositionFromTeam(team);
            Debug.Log(" In RPC : team id : " + teamId + " , actor id : " + photonView.Owner.ActorNumber);
            rig.transform.position = s.position;
            rig.transform.eulerAngles = s.rotation;
        }
    }

    public int getHealth()
    {
        return health;
    }
    
    void updateCdMapInfoCanvas()
    {
        if (infoCanvas != null)
            infoCanvas.updateCdText(spellBook.SpellCds);
    }

    void setInfoCanvas()
    {
        if (team == 0)
        {
            infoCanvas = GameObject.Find("InfoCanvasP1").GetComponent<InfoCanvas>();
        }
        else
        {
            infoCanvas = GameObject.Find("InfoCanvasP2").GetComponent<InfoCanvas>();
        }

        if (infoCanvas == null)
        {
            Debug.Log("could not set infocanvas with team " + team);
        }
    }

    public void updateHealthInfoCanvas(int enemyHealth)
    {
        if (infoCanvas != null)
            infoCanvas.updateHealth(health, enemyHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            if (!teamAssigned && InformationCenter.getPlayers().Count == GameSettings.nbPlayers)
            {
                teamAssigned = true;
                int id = photonView.Owner.ActorNumber;
                int t = GameSettings.getTeamWithId(id);
                Debug.Log("team id : " + t + " , actor id : " + id);
                photonView.RPC("setTeamRPC", RpcTarget.All, GameSettings.getTeamWithId(id));
            }

            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateHandAnimation(leftHandAnimator, leftHandPresence);
            UpdateHandAnimation(rightHandAnimator, rightHandPresence);

            // offset of -90 because the head is turned tower z axis at start and that the 
            // spells as defined as turned toward x axis
            leftHandPresence.HeadAngley = head.transform.rotation.eulerAngles.y - 90;
            rightHandPresence.HeadAngley = head.transform.rotation.eulerAngles.y - 90;

            if (Main.gameStarted && !Main.gameEnded)
            {
                spellBook.handleSpells(head, leftHandPresence, rightHandPresence);
                if (!leftHandPresence.Grabbing)
                    spellBook.handleShield(shield, leftHandPresence);
                if (!rightHandPresence.Grabbing)
                    spellBook.handleShield(shield, rightHandPresence);
                updateCdMapInfoCanvas();
            }

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

    [PunRPC]
    public void takeDamage(int damageAmount)
    {
        health = Mathf.Max(health - damageAmount, 0);
        CommunicationCenter.updateHealth();
    }

    public Vector3 getPosition()
    {
        return head.position;
    }

    public float getScaledHeadRadius()
    {
        // we take x because the head should be round
        Transform sphere = head.GetChild(0);
        return sphere.GetComponent<SphereCollider>().radius * sphere.localScale.x;
    }

    public int Team { get { return team; } }
    public bool Alive { get { return alive; } }
}
