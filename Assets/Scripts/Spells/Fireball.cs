using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fireball : Spell
{
    public const float FIREBALL_CD = 0.5f;
    public const int FIREBALL_SPEED = 250;
    public const int FIREBALL_DAMAGE = 50;


    private float fireBallAttractForce = 2.0f;
    private float forceTime = 0;
    private float FORCEFREQUENCY = 0.2f;
    private float decrementForceFactor = 0.9f;

    private int team = 0;

    private ArenaPlayer target;

    private void Awake()
    {
        cd = FIREBALL_CD;
        speed = FIREBALL_SPEED;
        damage = FIREBALL_DAMAGE;
    }

    void Update()
    {
        forceTime += Time.deltaTime;
        if (forceTime > FORCEFREQUENCY)
        {
            if (target != null)
            {
                Vector3 direction = target.getPosition() - transform.position;
                gameObject.GetComponent<Rigidbody>().AddForce(direction.normalized * fireBallAttractForce, ForceMode.Impulse);
                fireBallAttractForce *= decrementForceFactor;
            }
            forceTime = 0;
        }
    }

    public void setTeam(int iteam)
    {
        team = iteam;
    }

    public void setTarget(ArenaPlayer arenaPlayer)
    {
        target = arenaPlayer;
    }

    void OnCollisionEnter(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            playerTakeDamage(collision);
            dummyTakeDamage(collision.collider);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        ArenaPlayer player = getPlayerFromCollision(collision);
        if (player != null && player.Team != team)
        {
            player.photonView.RPC("takeDamage", RpcTarget.All, damage);
        }
    }

    void dummyTakeDamage(Collider collider)
    {
        Dummy dummy = collider.GetComponent<Dummy>();
        if (dummy != null)
        {
            dummy.takeDamage(damage, team);
        }
    }
}