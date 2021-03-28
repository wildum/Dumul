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
        if (photonView.IsMine)
        {
            playerTakeDamage(collision);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        NetworkPlayer player = getPlayerFromCollision(collision);
        if (player != null && player.Team != team)
        {
            player.photonView.RPC("takeDamage", RpcTarget.All, damage);
        }
        handleDestruction(collision, player);
    }

    void handleDestruction(Collision collision, NetworkPlayer player)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine &&
            (collision.collider.GetComponent<TerrainCollider>() != null ||
            // avoid self destruct of the cross
            (collision.collider.tag == "Player" && (player != null && team != player.Team)) ||
            collision.collider.tag == "Shield" ||
            collision.collider.tag == "Thunder" ||
            collision.collider.tag == "Arena"))
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
