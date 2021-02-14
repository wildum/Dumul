using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fireball : MonoBehaviour
{
    public const float CD_FIREBALL = 0.5f;
    public const int SPEED = 150;
    public const int DAMAGE = 50;

    private int ownerId = 0;

    public void setOwnerId(int iownerId)
    {
        ownerId = iownerId;
    }

    void OnCollisionEnter(Collision collision)
    {
        playerTakeDamage(collision);
        dummyTakeDamage(collision.collider);
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        NetworkPlayer player = getPlayerFromCollision(collision);
        if (player != null && player.getId() != ownerId)
        {
            player.takeDamage(DAMAGE);
        }
    }

    NetworkPlayer getPlayerFromCollision(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            GameObject g = collision.transform.parent.gameObject;
            if (g != null)
            {
                GameObject gp = g.transform.parent.gameObject;
                if (gp != null)
                {
                    return gp.GetComponent<NetworkPlayer>();
                }
            }
        }
        return null;
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