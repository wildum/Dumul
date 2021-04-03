using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fireball : Spell
{
    public const float FIREBALL_CD = 0.5f;
    public const int FIREBALL_SPEED = 250;
    public const int FIREBALL_DAMAGE = 50;

    private int team = 0;

    private void Awake()
    {
        cd = FIREBALL_CD;
        speed = FIREBALL_SPEED;
        damage = FIREBALL_DAMAGE;
    }

    public void setTeam(int iteam)
    {
        team = iteam;
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