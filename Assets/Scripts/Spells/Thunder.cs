using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Thunder : Spell
{
    public const float THUNDER_CD = 10f;
    public const float THUNDER_SCALE_SPEED = 0.8f;
    public const int THUNDER_DAMAGE = 150;
    public const float yStartingPosition = 6f;

    void Awake()
    {
        cd = THUNDER_CD;
        speed = 0;
        damage = THUNDER_DAMAGE;
    }

    void Update()
    {
        Vector3 v = transform.parent.transform.localScale;
        v.y += Time.deltaTime * THUNDER_SCALE_SPEED;
        transform.parent.transform.localScale = v;
    }

    void OnCollisionEnter(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            playerTakeDamage(collision);
            handleDestruction(collision);
        }
    }

    void playerTakeDamage(Collision collision)
    {
        NetworkPlayer player = getPlayerFromCollision(collision);
        if (player != null)
        {
            player.photonView.RPC("takeDamage", RpcTarget.All, damage);
        }
    }

    void handleDestruction(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine && 
            (collision.collider.GetComponent<TerrainCollider>() != null ||
            collision.collider.tag == "Player" ||
            collision.collider.tag == "Shield"))
        {
            PhotonNetwork.Destroy(gameObject);
            PhotonNetwork.Destroy(transform.parent.gameObject); ;
        }
    }
}
