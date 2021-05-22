using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Cross : Spell
{
    public const float CROSS_CD = 8.0f;
    public const int CROSS_SPEED = 10;
    public const int CROSS_DAMAGE = 100;

    private int team = 0;

    // needed because the cross has two collider so we dont want to trigger both when dealing damage
    private bool alreadyTookDamage = false;
    private bool alreadyDestroyed = false;

    private void Awake()
    {
        cd = CROSS_CD;
        speed = CROSS_SPEED;
        damage = CROSS_DAMAGE;
    }

    public void setTeam(int iteam)
    {
        team = iteam;
    }

    void OnCollisionEnter(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView != null && photonView.IsMine)
        {
            playerTakeDamage(collision);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        ArenaPlayer player = getPlayerFromCollision(collision);
        if (player != null && player.Team != team && !alreadyTookDamage)
        {
            alreadyTookDamage = true;
            player.photonView.RPC("takeDamage", RpcTarget.All, damage, playerId);
        }
        handleDestruction(collision, player);
    }

    void handleDestruction(Collision collision, ArenaPlayer player)
    {
        // no need to check photonView.IsMine because it is checked earlier
        if (!alreadyDestroyed && ((collision.collider.tag == "Player" && (player != null && team != player.Team)) ||
        collision.collider.tag == "Shield" ||
        collision.collider.tag == "Arena"))
        {
            PhotonNetwork.Destroy(gameObject);
            alreadyDestroyed = true;
        }
    }
}
