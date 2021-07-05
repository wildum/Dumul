using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArenaPlayer : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    public AudioSource hitmarker;

    public GameObject shield;

    public Material player11, player12, player21, player22;
    public Material handR11, handR12, handR21, handR22;
    public Material handL11, handL12, handL21, handL22;

    protected int health = GameSettings.PLAYER_HEALTH;

    protected InfoCanvas infoCanvas;

    protected int team = 1;
    protected bool alive = true;
    protected int idInTeam = 0;
    protected int id = 3;

    protected bool dashing = false;
    protected Vector3 dashTarget = new Vector3(0,0,0);

    private bool immortal = false;

    protected void setPlayerMaterials(int team, int idInTeam)
    {
        if (team == 0)
        {
            if (idInTeam == 0)
            {
                Debug.Log("set player11");
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material = player11;
                gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handL11;
                gameObject.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handR11;
            }
            else
            {
                Debug.Log("set player12");
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material = player12;
                gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handL12;
                gameObject.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handR12;
            }
        }
        else
        {
            if (idInTeam == 0)
            {
                Debug.Log("set player21");
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material = player21;
                gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handL21;
                gameObject.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handR21;
            }
            else
            {
                Debug.Log("set player22");
                gameObject.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material = player22;
                gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handL22;
                gameObject.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = handR22;
            }
        }
    }

    public int getHealth()
    {
        return health;
    }

    protected void setInfoCanvas()
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

    [PunRPC]
    public virtual void takeDamage(int damageAmount, int authorId)
    {
        ArenaPlayer p = InformationCenter.getPlayerById(authorId);
        if (p != null && p is NetworkPlayer && p.photonView.IsMine && id != authorId)
        {
            hitmarker.Play();
        }

        if (!immortal)
        {
            health = Mathf.Max(health - damageAmount, 0);
            if (health <= 0)
            {
                alive = false;
                if (photonView.IsMine)
                {
                    gameObject.SetActive(false);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            CommunicationCenter.updateHealth();
        }
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

    public void dash(int direction)
    {
        dashing = true;
        dashTarget = head.position + new Vector3(0, 0, direction);
    }

    public int Team { get { return team; } }
    public bool Alive { get { return alive; } }
    public int Id { get { return id; }}
    public bool Immortal { set { immortal = value;}}
    public int IdInTeam { get { return idInTeam; }}
}
