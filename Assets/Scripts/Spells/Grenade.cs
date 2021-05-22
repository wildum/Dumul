using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class Grenade : Spell
{
    public const float GRENADE_CD = 15.0f;
    public const int GRENADE_SPEED = 0;
    public const int GRENADE_DAMAGE = 100;

    private bool alive = true;

    private void Awake()
    {
        cd = GRENADE_CD;
        speed = GRENADE_SPEED;
        damage = GRENADE_DAMAGE;
    }

    void OnCollisionEnter(Collision collision)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine && alive)
        {
            alive = false;
            GameObject explosion = PhotonNetwork.Instantiate("Explosion", gameObject.transform.position, Quaternion.identity);
            explosion.GetComponent<Explosion>().Damage = GRENADE_DAMAGE;
            explosion.GetComponent<Explosion>().PlayerId = playerId;
            // should be in an RPC ?
            gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void onGrabLeave()
    {
        if (alive)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            Debug.Log("use gravity");
        }
    }
}
