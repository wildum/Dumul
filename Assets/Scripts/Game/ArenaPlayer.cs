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

    protected int health = GameSettings.PLAYER_HEALTH;

    protected GameObject shield;
    protected InfoCanvas infoCanvas;

    protected int team = 1;
    protected bool alive = true;

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

    public void updateHealthInfoCanvas(int enemyHealth)
    {
        if (infoCanvas != null)
            infoCanvas.updateHealth(health, enemyHealth);
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
