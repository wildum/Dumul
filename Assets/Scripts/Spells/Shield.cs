using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shield : Spell
{
    public const float SHIELD_CD = 6.0f;
    private bool active = false;
    private bool wasActive = false;
    private bool destroyed = false;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        Spell spell = collision.collider.GetComponent<Spell>();

        // special handling for the cross spell
        if (spell == null)
        {
            spell = collision.collider.transform.GetParentComponent<Spell>();
        }

        if (spell != null)
        {
            if (active)
            {
                updateShield(false);
                destroyed = true;
            }
        }
    }

    public void activate()
    {
        if (!active)
        {
            updateShield(true);
            wasActive = true;
        }
    }

    public void deactivate()
    {
        if (active)
        {
            updateShield(false);
        }
    }

    private void updateShield(bool iactive)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            active = iactive;
            gameObject.SetActive(iactive);
            gameObject.GetComponent<MeshRenderer>().enabled = iactive;
            photonView.RPC("updateActive", RpcTarget.Others, iactive);
        }
    }

    public bool isActive()
    {
        return active;
    }

    [PunRPC]
    public void updateActive(bool iactive)
    {
        active = iactive;
        gameObject.SetActive(iactive);
        gameObject.GetComponent<MeshRenderer>().enabled = iactive;
    }

    public bool WasActive { get {return wasActive;} set {wasActive = value;}}
    public bool Destroyed { get {return destroyed;} set {destroyed = value;}}

}
