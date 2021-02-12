﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fireball : MonoBehaviour
{
    public const float CD_FIREBALL = 0.5f;
    public const int SPEED = 100;
    public const int DAMAGE = 50;

    private int ownerId = 0;

    public void setOwnerId(int iownerId)
    {
        ownerId = iownerId;
    }

    void OnCollisionEnter(Collision collision)
    {
        playerTakeDamage(collision.collider);
        dummyTakeDamage(collision.collider);
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void playerTakeDamage(Collider collider)
    {
        NetworkPlayer player = collider.GetComponent<NetworkPlayer>();
        if (player != null && player.getId() != ownerId)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("sendDamageToPlayer", RpcTarget.All, player);
        }
    }

    [PunRPC]
    void sendDamageToPlayer(NetworkPlayer player)
    {
        player.takeDamage(DAMAGE);
    }

    void dummyTakeDamage(Collider collider)
    {
        Dummy dummy = collider.GetComponent<Dummy>();
        if (dummy != null)
        {
            dummy.takeDamage(DAMAGE, ownerId);
        }
    }
}