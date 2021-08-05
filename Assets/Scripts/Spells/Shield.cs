using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shield : Spell
{
    public const float SHIELD_CD = 6.0f;
    public const float SHIELD_RADIUS = 1.0f; // not connected to asset, if model changes then this should be updated
    public const int SHIELD_HEALTH = 30;
    private bool active = false;
    private bool wasActive = false;
    private bool destroyed = false;

    private int health = SHIELD_HEALTH;

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
            updateShieldStats(iactive);
            photonView.RPC("updateActive", RpcTarget.Others, iactive);
        }
    }

    public bool isActive()
    {
        return active;
    }

    private void updateShieldStats(bool iactive)
    {
        active = iactive;
        gameObject.SetActive(iactive);
        gameObject.GetComponent<MeshRenderer>().enabled = iactive;
        if (iactive)
        {
            health = SHIELD_HEALTH;
            updateColor();
        }
    }

    [PunRPC]
    public void updateActive(bool iactive)
    {
        updateShieldStats(iactive);
    }

    public void takeDamages(int amount)
    {
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            photonView.RPC("takeDamagesPun", RpcTarget.All, amount);
        }
    }

    void updateColor()
    {
        Color c = this.GetComponent<MeshRenderer>().material.color;
        c.a = 0.3f * (float) health / SHIELD_HEALTH;
        this.GetComponent<MeshRenderer>().material.color = c;
    }

    [PunRPC]
    private void takeDamagesPun(int amount)
    {
        health = Mathf.Max(0, health - amount);
        updateColor();
        if (health == 0)
        {
            updateActive(false);
        }
    }

    public bool WasActive { get {return wasActive;} set {wasActive = value;}}
    public bool Destroyed { get {return destroyed;} set {destroyed = value;}}

}
