using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : ArenaPlayer
{
    public AudioSource hurt;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    private HandPresence leftHandPresence;
    private HandPresence rightHandPresence;

    private bool teamAssigned = false;

    private XRRig rig;

    private SpellBook spellBook = new SpellBook();

    private bool initialised = false;

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    private void init()
    {
        if (!initialised)
        {
            if (photonView != null)
            {
                if (photonView.IsMine)
                {
                    rig = FindObjectOfType<XRRig>();
                    headRig = rig.transform.Find("Camera Offset/Main Camera");
                    leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
                    rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

                    leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<HandPresence>();
                    rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<HandPresence>();
                    foreach (var item in GetComponentsInChildren<Renderer>())
                    {
                        item.enabled = false;
                    }
                }
            }
            initialised = true;
        }
    }

    [PunRPC]
    public void setTeamAndIdsRPC(int teamId, int playerId, int iidInTeam)
    {
        init();
        team = teamId;
        idInTeam = iidInTeam;
        spellBook.Team = team;
        spellBook.setIdPlayerSpellCreator(playerId);
        if (leftHandPresence != null)
            leftHandPresence.Team = team;
        if (rightHandPresence != null)
            rightHandPresence.Team = team;
        id = playerId;
        setInfoCanvas();
    }

    [PunRPC]
    public override void takeDamage(int damageAmount, int authorId)
    {
        if (photonView.IsMine && gameObject != null)
        {
            hurt.Play();
        }
        base.takeDamage(damageAmount, authorId);
    }

    private void OnDestroy()
    {
        if (leftHandPresence != null)
            Destroy(leftHandPresence.gameObject);
        if (rightHandPresence != null)
            Destroy(rightHandPresence.gameObject);
    }
    
    void updateCdMapInfoCanvas()
    {
        if (infoCanvas != null)
            infoCanvas.updateCdText(spellBook.SpellCds);
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
                PlayerInfo playerInfo = GameSettings.getPlayerInfo(id);
                rig.transform.position = playerInfo.position.position;
                rig.transform.eulerAngles = playerInfo.position.rotation;
                photonView.RPC("setTeamAndIdsRPC", RpcTarget.All, playerInfo.team, playerInfo.id, playerInfo.idInTeam);
            }

            if (!dashing)
                MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateHandAnimation(leftHandAnimator, leftHandPresence);
            UpdateHandAnimation(rightHandAnimator, rightHandPresence);

            fixPositionInsideArena(head);

            // offset of -90 because the head is turned tower z axis at start and that the 
            // spells as defined as turned toward x axis
            leftHandPresence.HeadAngley = head.transform.rotation.eulerAngles.y - 90;
            rightHandPresence.HeadAngley = head.transform.rotation.eulerAngles.y - 90;

            if (Main.gameStarted && !Main.gameEnded)
            {
                spellBook.handleSpells(head, leftHandPresence, rightHandPresence);
                spellBook.handleShield(shield, leftHandPresence, rightHandPresence);
                shield.transform.position = head.position;
                updateCdMapInfoCanvas();
            }

        }
    }

    void FixedUpdate()
    {
        if (photonView != null && photonView.IsMine && dashing)
        {
            head.position = new Vector3(head.position.x, head.position.y, Mathf.Lerp(head.position.z, dashTarget.z, Time.deltaTime * SpellBook.DASH_SPEED));
            bool hitWall = fixPositionInsideArena(head);
            rig.transform.position = new Vector3(rig.transform.position.x, rig.transform.position.y, head.position.z);
            if (Mathf.Abs(head.position.z - dashTarget.z) < 0.1f || hitWall)
            {
                dashing = false;
            }
        }
    }


    bool fixPositionInsideArena(Transform t)
    {
        bool outOfBound = t.position.x <= -9 || t.position.x >= 9
            || t.position.y <= 0 || t.position.y >= 5.4
            || t.position.z <= -6 || t.position.z >= 6;
        if (outOfBound)
            t.position = new Vector3(Mathf.Clamp(t.position.x, -9, 9),Mathf.Clamp(t.position.y, 0, 5.4f), Mathf.Clamp(t.position.z, -6, 6));
        return outOfBound;
    }

    void UpdateHandAnimation(Animator handAnimator, HandPresence handPresence)
    {
        handAnimator.SetFloat("Trigger", handPresence.getTriggerValue());
        handAnimator.SetFloat("Grip", handPresence.getShieldValue());
    }

    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }
}
