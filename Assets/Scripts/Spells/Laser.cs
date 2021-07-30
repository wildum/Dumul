using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Laser : Spell
{
    public const float LASER_CD = 10f;
    public const float LASER_SCALE_SPEED = 2.0f;
    public const int LASER_DAMAGE = 10;
    public const float LASER_DURATION = 6.0f;
    public const float yStartingPosition = 5f;

    private const float TIME_BEFORE_SPAWN = 0.5f;
    private float t = 0;

    private bool canScale = true;

    private HandPresence hand;

    void Awake()
    {
        cd = LASER_CD;
        speed = 0;
        damage = LASER_DAMAGE;
    }

    void Update()
    {
        t += Time.deltaTime;
        handleLifeTime();
        handleScaling();
        handleRotation();
    }

    void handleScaling()
    {
        if (t > TIME_BEFORE_SPAWN && canScale)
        {
            Vector3 v = transform.parent.transform.localScale;
            v.y += Time.deltaTime * LASER_SCALE_SPEED;
            transform.parent.transform.localScale = v;
        }
    }

    void handleLifeTime()
    {
        if (t > LASER_DURATION || !hand.Channeling)
        {
            PhotonView photonView = PhotonView.Get(this);
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(transform.parent.gameObject);
            }
        }
    }

    void handleRotation()
    {
        if (hand != null)
        {
            gameObject.transform.rotation = hand.transform.rotation;
        }
    }

    void setHand(HandPresence ihand)
    {
        hand = ihand;
    }

    private void OnDestroy()
    {
        hand.Channeling = false;
    }

    void OnCollisionExit(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            handleCollision(collision, false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            //playerTakeDamage(collision);
            handleCollision(collision, true);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        ArenaPlayer player = getPlayerFromCollision(collision);
        if (player != null)
        {
            player.photonView.RPC("takeDamage", RpcTarget.All, damage, playerId);
        }
    }

    void handleCollision(Collision collision, bool touching)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            Debug.Log("Laser Touch!");
            if (collision.collider.tag == "Player" ||
                collision.collider.tag == "Shield")
            {
                canScale = !touching;
            }
            else 
            {
                canScale = true;
            }
        }
    }
}
